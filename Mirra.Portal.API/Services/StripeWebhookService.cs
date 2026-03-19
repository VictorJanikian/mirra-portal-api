using Microsoft.Extensions.Options;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;
using Stripe;
using Stripe.Checkout;
using Customer = Mirra_Portal_API.Model.Customer;

namespace Mirra_Portal_API.Services
{
    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISchedulingRepository _schedulingRepository;
        private readonly ICustomerPlatformConfigurationRepository _customerPlatformConfigurationRepository;
        private readonly ICronService _cronService;
        private readonly ISubscriptionPlanEvaluator _subscriptionPlanEvaluator;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<StripeWebhookService> _logger;
        private readonly ISubscriptionService _subscriptionService;

        public StripeWebhookService(
            ICustomerRepository customerRepository,
            ISchedulingRepository schedulingRepository,
            ICustomerPlatformConfigurationRepository customerPlatformConfigurationRepository,
            ICronService cronService,
            ISubscriptionPlanEvaluator subscriptionPlanEvaluator,
            IOptions<StripeSettings> stripeSettings,
            ILogger<StripeWebhookService> logger,
            ISubscriptionService subscriptionService)
        {
            _customerRepository = customerRepository;
            _schedulingRepository = schedulingRepository;
            _customerPlatformConfigurationRepository = customerPlatformConfigurationRepository;
            _cronService = cronService;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
            _subscriptionService = subscriptionService;
        }

        public async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null)
                throw new BadRequestException("Invalid checkout session data.");

            var customerEmail = session.CustomerEmail
                                ?? session.CustomerDetails?.Email;

            if (string.IsNullOrEmpty(customerEmail))
                throw new BadRequestException("Customer email not found in checkout session.");

            var customer = await _customerRepository.GetByEmail(customerEmail);
            if (customer == null)
                throw new NotFoundException($"Customer with email {customerEmail} not found.");

            int planId = await resolvePlanFromSession(session);

            customer.StripeCustomerId = session.CustomerId;
            customer.StripeSubscriptionId = session.SubscriptionId;
            customer.SubscriptionPlan = new SubscriptionPlan { Id = planId };
            customer.SubscriptionStatus = new SubscriptionStatus { Id = (int)ESubscriptionStatus.ACTIVE };

            await _customerRepository.Update(customer);

            _logger.LogInformation(
                "Checkout completed for customer {CustomerId}. Plan set to {PlanId}.",
                customer.Id, planId);
        }

        public async Task HandleInvoicePaymentSucceeded(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null)
                throw new BadRequestException("Invalid invoice data.");

            var customer = await _customerRepository.GetByEmail(invoice.CustomerEmail);
            if (customer == null)
            {
                _logger.LogWarning(
                    "Invoice payment succeeded for unknown customer {StripeCustomerId}.",
                    invoice.CustomerEmail);
                return;
            }

            int planId = await resolvePlanFromInvoice(invoice);

            _logger.LogInformation(
                "Beginning processing for plan {PlanId}.", planId);

            customer.StripeCustomerId = invoice.CustomerId;
            customer.StripeSubscriptionId = invoice.Parent.SubscriptionDetails.SubscriptionId;
            customer.SubscriptionPlan = new SubscriptionPlan { Id = planId };
            customer.SubscriptionStatus = new SubscriptionStatus { Id = (int)ESubscriptionStatus.ACTIVE };

            await _customerRepository.Update(customer);

            await reactivateSchedulingsIfCompliant(customer);

            _logger.LogInformation(
                "Invoice payment succeeded for customer {CustomerId}. Plan set to {PlanId}.",
                customer.Id, planId);
        }

        public async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            try
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                if (subscription == null)
                    throw new BadRequestException("Invalid subscription data.");

                var customer = await _customerRepository.GetByStripeCustomerId(subscription.CustomerId);
                if (customer == null)
                {
                    _logger.LogWarning(
                        "Subscription updated for unknown Stripe customer {StripeCustomerId}.",
                        subscription.CustomerId);
                    return;
                }

                int planId = await resolvePlanFromSubscription(subscription);

                customer.StripeSubscriptionId = subscription.Id;
                customer.SubscriptionPlan = new SubscriptionPlan { Id = planId };
                customer.SubscriptionStatus = new SubscriptionStatus { Id = (int)ESubscriptionStatus.ACTIVE };

                await _customerRepository.Update(customer);

                await reactivateSchedulingsIfCompliant(customer);
                await suspendNonCompliantSchedulings(customer, ESchedulingStatus.SUSPENDED_DUE_TO_PLAN_DOWNGRADE);

                _logger.LogInformation(
                    "Subscription updated for customer {CustomerId}. Plan set to {PlanId}.",
                    customer.Id, planId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
        }

        public async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null)
                throw new BadRequestException("Invalid subscription data.");

            var customer = await _customerRepository.GetByStripeCustomerId(subscription.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning(
                    "Subscription deleted for unknown Stripe customer {StripeCustomerId}.",
                    subscription.CustomerId);
                return;
            }

            customer.StripeCustomerId = subscription.CustomerId;
            customer.StripeSubscriptionId = subscription.Id;
            customer.SubscriptionPlan = new SubscriptionPlan { Id = (int)ESubscriptionPlan.FREE };
            customer.SubscriptionStatus = new SubscriptionStatus { Id = (int)ESubscriptionStatus.CANCELLED };

            await _customerRepository.Update(customer);

            await suspendNonCompliantSchedulings(customer, ESchedulingStatus.SUSPENDED_DUE_TO_PLAN_DOWNGRADE);

            _logger.LogInformation(
                "Subscription deleted for customer {CustomerId}. Plan set to FREE.",
                customer.Id);
        }


        public async Task HandlePaymentFailed(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null)
                throw new BadRequestException("Invalid subscription data.");

            var customer = await _customerRepository.GetByStripeCustomerId(invoice.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning(
                    "Subscription deleted for unknown Stripe customer {StripeCustomerId}.",
                    invoice.CustomerId);
                return;
            }

            customer.StripeCustomerId = invoice.CustomerId;
            customer.SubscriptionPlan = new SubscriptionPlan { Id = (int)ESubscriptionPlan.FREE };
            customer.SubscriptionStatus = new SubscriptionStatus { Id = (int)ESubscriptionStatus.PAYMENT_FAILED };

            await _customerRepository.Update(customer);

            await suspendNonCompliantSchedulings(customer, ESchedulingStatus.SUSPENDED_DUE_TO_LACK_PAYMENT);

            _logger.LogInformation(
                "Payment failed event processed for customer {CustomerId}. Plan set to FREE.",
                customer.Id);
        }

        private async Task<int> resolvePlanFromSession(Session session)
        {

            if (session.AmountTotal.HasValue)
            {
                var plan = await _subscriptionService.GetSubscriptionPlanByPrice((int)session.AmountTotal.Value);
                if (plan != null) return plan.Id;
            }

            throw new BadRequestException(
                "Could not determine subscription plan from checkout session.");
        }

        private async Task<int> resolvePlanFromInvoice(Invoice invoice)
        {
            if (invoice != null)
            {
                var plan = await _subscriptionService.GetSubscriptionPlanByPrice((int)invoice.AmountPaid);
                if (plan != null) return plan.Id;
            }

            throw new BadRequestException(
                "Could not determine subscription plan from invoice.");
        }

        private async Task<int> resolvePlanFromSubscription(Subscription subscription)
        {
            if (subscription.Items?.Data != null)
            {
                foreach (var item in subscription.Items.Data)
                {
                    if (item.Price != null)
                    {
                        var plan = await _subscriptionService.GetSubscriptionPlanByPrice((int)item.Price.UnitAmount.GetValueOrDefault());
                        if (plan != null) return plan.Id;
                    }
                }
            }

            throw new BadRequestException(
                "Could not determine subscription plan from subscription.");
        }

        private async Task suspendNonCompliantSchedulings(Customer customer, ESchedulingStatus newSchedulingStatus)
        {
            var updatedCustomer = await _customerRepository.GetById(customer.Id);
            var schedulings = await _schedulingRepository.GetAllByCustomerId(customer.Id);
            var configurations = await _customerPlatformConfigurationRepository.GetAllForCustomer(customer.Id);
            var numberOfPlatformsConnectedAreAllowedInCustomerCurrentPlan = await _subscriptionPlanEvaluator
                    .checkIfNumberOfConfigurationsAreAllowedInCustomerCurrentPlan(updatedCustomer, configurations.Count());

            if (!numberOfPlatformsConnectedAreAllowedInCustomerCurrentPlan)
            {
                foreach (var scheduling in schedulings)
                    await suspendScheduling(updatedCustomer, scheduling, newSchedulingStatus);
            }

            else
            {
                foreach (var configuration in configurations)
                {
                    var isAllowed = await checkIfConfigurationNumberOfPostsAreCompliantToCustomerPlan(updatedCustomer, configuration);

                    if (!isAllowed)
                        await suspendSchedulings(updatedCustomer, configuration.Schedulings, newSchedulingStatus);
                }
            }
        }

        private async Task suspendSchedulings(Customer customer, List<Scheduling> schedulings, ESchedulingStatus newSchedulingStatus)
        {
            foreach (var scheduling in schedulings)
            {
                if (scheduling.SchedulingStatus?.Id != (int)newSchedulingStatus)
                    await suspendScheduling(customer, scheduling, newSchedulingStatus);
            }
        }

        private async Task suspendScheduling(Customer customer, Scheduling scheduling, ESchedulingStatus newSchedulingStatus)
        {
            scheduling.SchedulingStatus = new SchedulingStatus
            {
                Id = (int)newSchedulingStatus
            };
            await _schedulingRepository.Update(scheduling);

            _logger.LogInformation(
                "Scheduling {SchedulingId} suspended for customer {CustomerId} due to plan downgrade.",
                scheduling.Id, customer.Id);
        }

        private async Task reactivateSchedulingsIfCompliant(Customer customer)
        {
            var updatedCustomer = await _customerRepository.GetById(customer.Id);
            var schedulings = await _schedulingRepository.GetAllByCustomerId(customer.Id);
            var configurations = await _customerPlatformConfigurationRepository.GetAllForCustomer(customer.Id);
            var numberOfPlatformsConnectedAreAllowedInCustomerCurrentPlan = await _subscriptionPlanEvaluator
                    .checkIfNumberOfConfigurationsAreAllowedInCustomerCurrentPlan(updatedCustomer, configurations.Count());

            if (numberOfPlatformsConnectedAreAllowedInCustomerCurrentPlan)
            {

                foreach (var configuration in configurations)
                {
                    var isAllowed = await checkIfConfigurationNumberOfPostsAreCompliantToCustomerPlan(updatedCustomer, configuration);

                    if (isAllowed)
                        await activateSchedulings(updatedCustomer, configuration.Schedulings);
                }
            }
        }

        private async Task<bool> checkIfConfigurationNumberOfPostsAreCompliantToCustomerPlan(Customer customer, CustomerPlatformConfiguration configuration)
        {
            int totalRunsPerWeek = 0;

            foreach (var schedule in configuration.Schedulings)
                totalRunsPerWeek += schedule.RunsPerWeek;

            return await _subscriptionPlanEvaluator
                        .checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(customer, totalRunsPerWeek);

        }

        private async Task activateSchedulings(Customer customer, List<Scheduling> schedulings)
        {
            foreach (var scheduling in schedulings)
            {
                await activateScheduling(customer, scheduling);
            }
        }

        private async Task activateScheduling(Customer customer, Scheduling scheduling)
        {
            scheduling.SchedulingStatus = new SchedulingStatus
            {
                Id = (int)ESchedulingStatus.ACTIVE
            };
            await _schedulingRepository.Update(scheduling);

            _logger.LogInformation(
                "Scheduling {SchedulingId} reactivated for customer {CustomerId} after payment.",
                scheduling.Id, customer.Id);
        }
    }
}
