using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Helper;
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
        ICronService _cronService;

        public ConfigurationService(ICustomerPlatformConfigurationRepository configurationRepository,
                                    IdentityHelper identityHelper,
                                    SymmetricEncryptionHelper symmetricEncryptionHelper,
                                    ISchedulingRepository schedulingRepository,
                                    ICustomerRepository customerRepository,
                                    ISubscriptionPlanEvaluator subscriptionPlanEvaluator,
                                    ICronService cronService)
        {
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
            _symmetricEncryptionHelper = symmetricEncryptionHelper;
            _schedulingRepository = schedulingRepository;
            _customerRepository = customerRepository;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
            _cronService = cronService;
        }

        public async Task<CustomerPlatformConfiguration> CreateConfiguration(CustomerPlatformConfiguration configuration)
        {
            validateIntervalsFormat(configuration);

            var customer = await _customerRepository.GetById(_identityHelper.UserId());
            var customerConfigurations = await _configurationRepository.GetAllForCustomer(_identityHelper.UserId());

            await validateIfNumberOfConnectionsExceedsMaximumAllowedForCustomer(customer, customerConfigurations);
            await validateIfIntervalsExceedMaximumAllowedForCustomer(customer, configuration);

            configuration.Customer = new Customer { Id = _identityHelper.UserId() };
            configuration.Password = _symmetricEncryptionHelper.Encrypt(configuration.Password);

            foreach (var schedule in configuration.Schedulings)
            {
                schedule.RunsPerWeek = _cronService.CalculateMaxRunsPerWeek(schedule.Interval);
                schedule.SchedulingStatus = new SchedulingStatus { Id = (int)ESchedulingStatus.ACTIVE };
            }

            return await _configurationRepository.Create(configuration);
        }

        public async Task<Scheduling> CreateSchedule(int configurationId, Scheduling scheduling)
        {
            validateIntervalFormat(scheduling);
            await checkIfConfigurationBelongsToCustomer(configurationId);
            scheduling.RunsPerWeek = _cronService.CalculateMaxRunsPerWeek(scheduling.Interval);
            var customer = await getCustomerByConfigurationId(configurationId);
            await validateIfIntervalExceedMaximumAllowedForCustomer(customer, configurationId, scheduling);
            scheduling.CustomerPlatformConfiguration = new CustomerPlatformConfiguration { Id = configurationId };
            scheduling.SchedulingStatus = new SchedulingStatus { Id = (int)ESchedulingStatus.ACTIVE };
            return await _schedulingRepository.Create(scheduling);
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

        public async Task<Scheduling> GetSchedule(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            var scheduling = await _schedulingRepository.GetById(schedulingId);
            if (scheduling == null || scheduling.CustomerPlatformConfiguration.Id != configurationId)
                throw new BadRequestException("Scheduling not found.");
            return scheduling;
        }


        public async Task<Scheduling> UpdateSchedule(int configurationId, int schedulingId, Scheduling scheduling)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            validateIntervalFormat(scheduling);

            var customer = await _customerRepository.GetById(_identityHelper.UserId());
            var configuration = await _configurationRepository.GetById(configurationId);
            scheduling.RunsPerWeek = _cronService.CalculateMaxRunsPerWeek(scheduling.Interval);

            configuration.Schedulings.Where(s => s.Id == schedulingId).First().Interval = scheduling.Interval;
            await validateIfIntervalsExceedMaximumAllowedForCustomer(customer, configuration);

            scheduling.Id = schedulingId;
            return await _schedulingRepository.Update(scheduling);
        }

        public async Task DeleteSchedule(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            await _schedulingRepository.Delete(schedulingId);
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

        /*---*/

        private void validateIntervalsFormat(CustomerPlatformConfiguration configuration)
        {
            _cronService.ValidateIntervalsFormat(configuration);
        }

        private void validateIntervalFormat(Scheduling scheduling)
        {
            _cronService.ValidateIntervalFormat(scheduling);
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

        private async Task validateIfIntervalExceedMaximumAllowedForCustomer(Customer customer, int configurationId, Scheduling newSchedule)
        {
            var configuration = await _configurationRepository.GetById(configurationId);
            int totalRunsPerWeek = await calculateTotalRunsPerWeekForConfiguration(configuration);

            totalRunsPerWeek += _cronService.CalculateMaxRunsPerWeek(newSchedule.Interval);

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

        private async Task checkIfSchedulingBelongsToConfiguration(int configurationId, int schedulingId)
        {
            var scheduling = await _schedulingRepository.GetById(schedulingId);
            if (scheduling == null || scheduling.CustomerPlatformConfiguration.Id != configurationId)
                throw new NotFoundException("Scheduling not found.");
        }

        private async Task<Customer> getCustomerByConfigurationId(int configurationId)
        {
            return await _customerRepository.GetByConfigurationId(configurationId);
        }

    }
}
