using Cronos;
using Mirra_Portal_API.Database.Repositories.Interfaces;
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
        IdentityHelper _identityHelper;
        SymmetricEncryptionHelper _symmetricEncryptionHelper;

        public ConfigurationService(ICustomerPlatformConfigurationRepository configurationRepository,
                                    IdentityHelper identityHelper,
                                    SymmetricEncryptionHelper symmetricEncryptionHelper,
                                    ISchedulingRepository schedulingRepository)
        {
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
            _symmetricEncryptionHelper = symmetricEncryptionHelper;
            _schedulingRepository = schedulingRepository;
        }

        public async Task<CustomerPlatformConfiguration> CreateConfiguration(CustomerPlatformConfiguration configuration)
        {
            validateIntervals(configuration);
            configuration.Customer = new Customer { Id = _identityHelper.UserId() };
            configuration.Password = _symmetricEncryptionHelper.Encrypt(configuration.Password);
            return await _configurationRepository.Create(configuration);
        }


        public async Task<Scheduling> CreateScheduling(int configurationId, Scheduling scheduling)
        {
            validateInterval(scheduling);
            await checkIfConfigurationBelongsToCustomer(configurationId);
            scheduling.CustomerPlatformConfiguration = new CustomerPlatformConfiguration { Id = configurationId };
            return await _schedulingRepository.Create(scheduling);
        }

        private void validateIntervals(CustomerPlatformConfiguration configuration)
        {
            foreach (var schedule in configuration.Schedulings)
            {
                if (!CronExpression.TryParse(schedule.Interval, CronFormat.Standard, out _))
                    throw new BadRequestException("Interval must be a 5 fields valid cron expression (ex.: \"0 2 * * 1\").");
            }
        }

        private void validateInterval(Scheduling scheduling)
        {
            if (!CronExpression.TryParse(scheduling.Interval, CronFormat.Standard, out _))
                throw new BadRequestException("Interval must be a 5 fields valid cron expression (ex.: \"0 2 * * 1\").");
        }

        public async Task<List<Scheduling>> GetConfigurationSchedulings(int configurationId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            return await _schedulingRepository.GetAllByConfigurationId(configurationId);
        }

        public async Task<Scheduling> GetScheduling(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            var scheduling = await _schedulingRepository.GetById(schedulingId);
            if (scheduling == null || scheduling.CustomerPlatformConfiguration.Id != configurationId)
                throw new BadRequestException("Scheduling not found.");
            return scheduling;
        }


        public async Task<Scheduling> UpdateScheduling(int configurationId, int schedulingId, Scheduling scheduling)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            scheduling.Id = schedulingId;
            return await _schedulingRepository.Update(scheduling);
        }

        public async Task DeleteScheduling(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            await checkIfSchedulingBelongsToConfiguration(configurationId, schedulingId);
            await _schedulingRepository.Delete(schedulingId);
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

        public async Task<List<CustomerPlatformConfiguration>> GetAllConfigurations()
        {
            return await _configurationRepository.GetAllForCustomer(_identityHelper.UserId());
        }
    }
}
