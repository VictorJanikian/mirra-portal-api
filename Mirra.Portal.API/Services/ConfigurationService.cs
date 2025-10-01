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
        ICustomerContentPlatformConfigurationRepository _configurationRepository;
        ISchedulingRepository _schedulingRepository;
        IdentityHelper _identityHelper;
        SymmetricEncryptionHelper _symmetricEncryptionHelper;

        public ConfigurationService(ICustomerContentPlatformConfigurationRepository configurationRepository,
                                    IdentityHelper identityHelper,
                                    SymmetricEncryptionHelper symmetricEncryptionHelper,
                                    ISchedulingRepository schedulingRepository)
        {
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
            _symmetricEncryptionHelper = symmetricEncryptionHelper;
            _schedulingRepository = schedulingRepository;
        }

        public async Task<CustomerContentPlatformConfiguration> CreateConfiguration(CustomerContentPlatformConfiguration configuration)
        {
            validateIntervals(configuration);
            configuration.Customer = new Customer { Id = _identityHelper.UserId() };
            configuration.Password = _symmetricEncryptionHelper.Encrypt(configuration.Password);
            return await _configurationRepository.Create(configuration);
        }


        private void validateIntervals(CustomerContentPlatformConfiguration configuration)
        {
            foreach (var schedule in configuration.Schedulings)
            {
                if (!CronExpression.TryParse(schedule.Interval, CronFormat.Standard, out _))
                    throw new BadRequestException("Interval must be a 5 fields valid cron expression (ex.: \"0 2 * * 1\").");
            }
        }

        public async Task<List<Scheduling>> GetConfigurationSchedulings(int configurationId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            return await _schedulingRepository.GetAllByConfigurationId(configurationId);
        }

        public async Task<Scheduling> GetScheduling(int configurationId, int schedulingId)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            var scheduling = await _schedulingRepository.GetById(schedulingId);
            if (scheduling == null || scheduling.CustomerContentPlatformConfiguration.Id != configurationId)
                throw new BadRequestException("Scheduling not found.");
            return scheduling;
        }


        private async Task checkIfConfigurationBelongsToCustomer(int configurationId)
        {
            var configuration = await _configurationRepository.GetById(configurationId);
            if (configuration == null || configuration.Customer.Id != _identityHelper.UserId())
                throw new BadRequestException("Configuration not found.");
        }

        public async Task<Scheduling> UpdateScheduling(int configurationId, int schedulingId, Scheduling scheduling)
        {
            await checkIfConfigurationBelongsToCustomer(configurationId);
            scheduling.Id = schedulingId;
            return await _schedulingRepository.Update(scheduling);
        }
    }
}
