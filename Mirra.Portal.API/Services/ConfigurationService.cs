using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Integration.Interfaces;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class ConfigurationService : IConfigurationService
    {
        ICustomerPlatformConfigurationRepository _configurationRepository;
        ISchedulingRepository _schedulingRepository;
        ICustomerRepository _customerRepository;
        IdentityHelper _identityHelper;
        SymmetricEncryptionHelper _symmetricEncryptionHelper;
        ISubscriptionPlanEvaluator _subscriptionPlanEvaluator;
        IWordpressIntegration _wordpressIntegration;
        ICronService _cronService;

        public ConfigurationService(ICustomerPlatformConfigurationRepository configurationRepository,
                                    IdentityHelper identityHelper,
                                    SymmetricEncryptionHelper symmetricEncryptionHelper,
                                    ISchedulingRepository schedulingRepository,
                                    ICustomerRepository customerRepository,
                                    ISubscriptionPlanEvaluator subscriptionPlanEvaluator,
                                    ICronService cronService,
                                    IWordpressIntegration wordpressIntegration)
        {
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
            _symmetricEncryptionHelper = symmetricEncryptionHelper;
            _schedulingRepository = schedulingRepository;
            _customerRepository = customerRepository;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
            _cronService = cronService;
            _wordpressIntegration = wordpressIntegration;
        }

        public async Task<CustomerPlatformConfiguration> CreateConfiguration(CustomerPlatformConfiguration configuration)
        {
            validateIntervalsFormat(configuration);
            await CheckIfIsValidWordPressSite(configuration.Url);

            var customer = await _customerRepository.GetById(_identityHelper.UserId());
            var customerConfigurations = await _configurationRepository.GetAllForCustomer(_identityHelper.UserId());

            await validateIfNumberOfConnectionsExceedsMaximumAllowedForCustomer(customer, customerConfigurations);
            await validateIfIntervalsExceedMaximumAllowedForCustomer(customer, configuration);

            configuration.Customer = new Customer { Id = _identityHelper.UserId() };
            configuration.Password = _symmetricEncryptionHelper.Encrypt(configuration.Password);

            foreach (var schedule in configuration.Schedulings)
            {
                schedule.Interval = _cronService.ConvertCronToUtc(schedule.Interval, schedule.Timezone);
                schedule.RunsPerWeek = _cronService.CalculateMaxRunsPerWeek(schedule.Interval);
                schedule.SchedulingStatus = new SchedulingStatus { Id = (int)ESchedulingStatus.ACTIVE };
            }

            configuration.Url = configuration.Url.TrimEnd('/') + "/wp-json";

            return await _configurationRepository.Create(configuration);
        }

        private async Task CheckIfIsValidWordPressSite(string url)
        {
            await _wordpressIntegration.checkIfIsValidWordPressSite(url);
        }

        public async Task<CustomerPlatformConfiguration> GetConfiguration(int configurationId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);

            var customer = await _customerRepository.GetById(_identityHelper.UserId());
            var configuration = await _configurationRepository.GetById(configurationId);
            int totalRunsPerWeek = await calculateTotalRunsPerWeekForConfiguration(configuration);
            var remainingRunsPerWeek = await _subscriptionPlanEvaluator.getRemainingRunsPerWeekAllowed(customer, configurationId, totalRunsPerWeek);
            configuration.RemainingRunsPerWeek = remainingRunsPerWeek ?? -1;
            return configuration;
        }




        public async Task<List<CustomerPlatformConfiguration>> GetAllConfigurations()
        {
            var customer = await _customerRepository.GetById(_identityHelper.UserId());
            var configurations = await _configurationRepository.GetAllForCustomer(_identityHelper.UserId());

            foreach (var configuration in configurations)
            {
                var totalRunsPerWeek = await calculateTotalRunsPerWeekForConfiguration(configuration);
                var remainingRunsPerWeek = await _subscriptionPlanEvaluator.getRemainingRunsPerWeekAllowed(customer, configuration.Id, totalRunsPerWeek);
                configuration.RemainingRunsPerWeek = remainingRunsPerWeek ?? -1;
            }

            return configurations;
        }

        public async Task<bool> HasSuspendedSchedulingsDueToLackOfPayment()
        {
            return await _schedulingRepository.HasAnyByCustomerIdAndStatus(
                _identityHelper.UserId(),
                ESchedulingStatus.SUSPENDED_DUE_TO_LACK_PAYMENT);
        }

        public async Task<bool> HasSuspendedSchedulingsDueToPlanDowngrade()
        {
            return await _schedulingRepository.HasAnyByCustomerIdAndStatus(
                _identityHelper.UserId(),
                ESchedulingStatus.SUSPENDED_DUE_TO_PLAN_DOWNGRADE);
        }


        public async Task<bool> HasSuspendedSchedulingsDueToLackOfPayment(int configurationId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);

            return await _schedulingRepository.HasAnyByConfigurationIdAndStatus(
                            configurationId,
                            ESchedulingStatus.SUSPENDED_DUE_TO_LACK_PAYMENT);

        }

        public async Task<bool> HasSuspendedSchedulingsDueToPlanDowngrade(int configurationId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);

            return await _schedulingRepository.HasAnyByConfigurationIdAndStatus(
                        configurationId,
                        ESchedulingStatus.SUSPENDED_DUE_TO_PLAN_DOWNGRADE);
        }

        public async Task DeleteConfiguration(int configurationId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await _configurationRepository.Delete(configurationId);
        }

        public async Task<CustomerPlatformConfiguration> UpdateConfiguration(int configurationId, CustomerPlatformConfiguration configuration)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);

            configuration.Id = configurationId;
            configuration.Password = _symmetricEncryptionHelper.Encrypt(configuration.Password);

            return await _configurationRepository.Update(configuration);
        }

        /*---*/

        private void validateIntervalsFormat(CustomerPlatformConfiguration configuration)
        {
            _cronService.ValidateIntervalsFormat(configuration);
        }



        private async Task validateIfIntervalsExceedMaximumAllowedForCustomer(Customer customer, CustomerPlatformConfiguration configuration)
        {

            if (configuration.Schedulings == null) return;

            int totalRunsPerWeek = await calculateTotalRunsPerWeekForConfiguration(configuration);

            var isAllowed = await _subscriptionPlanEvaluator
                .checkIfRunsPerWeekAreAllowedInCustomerCurrentPlan(customer, totalRunsPerWeek);

            if (!isAllowed)
                throw new SubscriptionException("Your current plan does not allow the intended number of runs per week.");
        }


        private async Task<int> calculateTotalRunsPerWeekForConfiguration(CustomerPlatformConfiguration configuration)
        {
            return await _cronService.CalculateTotalRunsPerWeekForConfiguration(configuration);
        }


        private async Task validateIfNumberOfConnectionsExceedsMaximumAllowedForCustomer(Customer customer, List<CustomerPlatformConfiguration> customerConfigurations)
        {
            var numberOfPlatformsConnectedAreAllowedInCustomerCurrentPlan = await _subscriptionPlanEvaluator
                            .checkIfNumberOfConfigurationsAreAllowedInCustomerCurrentPlan(customer, customerConfigurations.Count() + 1);
            if (!numberOfPlatformsConnectedAreAllowedInCustomerCurrentPlan)
                throw new BadRequestException("The number of platforms connected exceeds the limit of your current subscription plan.");
        }


        private async Task checkIfConfigurationBelongsToCustomer(int configurationId)
        {
            var configuration = await _configurationRepository.GetById(configurationId);
            if (configuration == null || configuration.Customer.Id != _identityHelper.UserId())
                throw new NotFoundException("Configuration not found.");
        }

    }
}
