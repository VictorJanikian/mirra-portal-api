using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class SubscriptionService : ISubscriptionService
    {

        private readonly ISubscriptionPlanRepository _subscriptionRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPaymentLinkRepository _subscriptionPaymentLinkRepository;
        private readonly ICustomerPlatformConfigurationRepository _configurationRepository;
        private readonly ISubscriptionPlanEvaluator _subscriptionPlanEvaluator;
        IdentityHelper _identityHelper;

        public SubscriptionService(ISubscriptionPlanRepository subscriptionRepository,
            ISubscriptionPlanEvaluator subscriptionPlanEvaluator,
            ICustomerRepository customerRepository,
            ISubscriptionPaymentLinkRepository subscriptionPaymentLinkRepository,
            ICustomerPlatformConfigurationRepository configurationRepository,
            IdentityHelper identityHelper)
        {
            _subscriptionRepository = subscriptionRepository;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
            _configurationRepository = configurationRepository;
            _customerRepository = customerRepository;
            _subscriptionPaymentLinkRepository = subscriptionPaymentLinkRepository;
            _identityHelper = identityHelper;

        }

        public async Task<SubscriptionPlan> GetSubscriptionPlanByPriceAndCountry(int price, string country)
        {
            var countrySpecificPaymentLink = await _subscriptionPaymentLinkRepository.GetByPriceAndCountry(price, country);
            if (countrySpecificPaymentLink != null)
                return await _subscriptionRepository.GetById(countrySpecificPaymentLink.SubscriptionPlan.Id);
            else
                return await _subscriptionRepository.GetByPrice(price);

        }

        public async Task<List<SubscriptionPlan>> GetAllSubscriptionPlans()
        {
            var subscriptionPlans = await _subscriptionRepository.GetAll();
            var customer = await _customerRepository.GetById(_identityHelper.UserId());

            foreach (var subscriptionPlan in subscriptionPlans)
            {
                var countrySpecificPaymentLink = await _subscriptionPaymentLinkRepository.GetBySubscriptionPlanIdAndCountry(subscriptionPlan.Id, customer.Country);
                if (countrySpecificPaymentLink != null)
                    subscriptionPlan.PaymentLink = countrySpecificPaymentLink.PaymentLink;
                else
                    subscriptionPlan.PaymentLink = subscriptionPlan.DefaultPaymentLink;

            }

            return subscriptionPlans;

        }

        public async Task<int> GetRemainingConfigurationsAllowed(int customerId)
        {
            var customer = await _customerRepository.GetById(customerId);
            var customerConfigurations = await _configurationRepository.GetAllForCustomer(customerId);
            var reamining = await _subscriptionPlanEvaluator.getRemainingConfigurationsAllowed(customer, customerConfigurations.Count);
            return reamining ?? -1;

        }
    }
}
