using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Enums;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class ScheduleService : IScheduleService
    {
        ICustomerPlatformConfigurationRepository _configurationRepository;
        ISubscriptionPlanEvaluator _subscriptionPlanEvaluator;
        ISchedulingRepository _schedulingRepository;
        ICronService _cronService;
        ICustomerRepository _customerRepository;
        IdentityHelper _identityHelper;

        public ScheduleService(ICustomerPlatformConfigurationRepository configurationRepository,
            ISchedulingRepository schedulingRepository, ICronService cronService,
            ISubscriptionPlanEvaluator subscriptionPlanEvaluator, ICustomerRepository customerRepository,
            IdentityHelper identityHelper)
        {
            _configurationRepository = configurationRepository;
            _schedulingRepository = schedulingRepository;
            _cronService = cronService;
            _subscriptionPlanEvaluator = subscriptionPlanEvaluator;
            _customerRepository = customerRepository;
            _identityHelper = identityHelper;
        }

        public async Task<Scheduling> CreateSchedule(int configurationId, Scheduling scheduling)
        {
            validateIntervalFormat(scheduling);
            scheduling.Interval = _cronService.ConvertCronToUtc(scheduling.Interval, scheduling.Timezone);
            await checkIfConfigurationBelongsToCustomer(configurationId);
            scheduling.RunsPerWeek = _cronService.CalculateMaxRunsPerWeek(scheduling.Interval);
            var customer = await getCustomerByConfigurationId(configurationId);
            await validateIfIntervalExceedMaximumAllowedForCustomer(customer, configurationId, scheduling);
            scheduling.CustomerPlatformConfiguration = new CustomerPlatformConfiguration { Id = configurationId };
            scheduling.SchedulingStatus = new SchedulingStatus { Id = (int)ESchedulingStatus.ACTIVE };
            return await _schedulingRepository.Create(scheduling);
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
            scheduling.Interval = _cronService.ConvertCronToUtc(scheduling.Interval, scheduling.Timezone);

            var customer = await _customerRepository.GetById(_identityHelper.UserId());
            var configuration = await _configurationRepository.GetById(configurationId);
            scheduling.RunsPerWeek = _cronService.CalculateMaxRunsPerWeek(scheduling.Interval);

            configuration.Schedulings.Where(s => s.Id == schedulingId).First().Interval = scheduling.Interval;
            await validateIfIntervalsExceedMaximumAllowedForCustomer(customer, configuration);
            await reactivateAllSchedules(configuration);
            scheduling.Id = schedulingId;
            return await _schedulingRepository.Update(scheduling);
        }

        public async Task DeleteSchedule(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            await _schedulingRepository.Delete(schedulingId);
        }

        private void validateIntervalFormat(Scheduling scheduling)
        {
            _cronService.ValidateIntervalFormat(scheduling);
        }


        private async Task reactivateAllSchedules(CustomerPlatformConfiguration configuration)
        {
            foreach (var schedule in configuration.Schedulings)
            {
                if (schedule.SchedulingStatus.Id != (int)ESchedulingStatus.ACTIVE)
                {
                    schedule.SchedulingStatus = new SchedulingStatus { Id = (int)ESchedulingStatus.ACTIVE };
                    await _schedulingRepository.Update(schedule);
                }
            }
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

        private async Task checkIfConfigurationBelongsToCustomer(int configurationId)
        {
            var configuration = await _configurationRepository.GetById(configurationId);
            if (configuration == null || configuration.Customer.Id != _identityHelper.UserId())
                throw new NotFoundException("Configuration not found.");
        }

        private async Task<int> calculateTotalRunsPerWeekForConfiguration(CustomerPlatformConfiguration configuration)
        {
            return await _cronService.CalculateTotalRunsPerWeekForConfiguration(configuration);
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

    }
}
