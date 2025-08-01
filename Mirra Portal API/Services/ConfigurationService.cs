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
        IdentityHelper _identityHelper;
        SymmetricEncryptionHelper _symmetricEncryptionHelper;

        public ConfigurationService(ICustomerContentPlatformConfigurationRepository configurationRepository,
                                    IdentityHelper identityHelper,
                                    SymmetricEncryptionHelper symmetricEncryptionHelper)
        {
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
            _symmetricEncryptionHelper = symmetricEncryptionHelper;
        }

        public async Task<CustomerPlatformConfiguration> CreateConfiguration(CustomerPlatformConfiguration configuration)
        {
            validateIntervals(configuration);
            configuration.Customer = new Customer { Id = _identityHelper.UserId() };
            configuration.Password = _symmetricEncryptionHelper.Encrypt(configuration.Password);
            return await _configurationRepository.Create(configuration);
        }

        private void validateIntervals(CustomerPlatformConfiguration configuration)
        {
            foreach (var schedule in configuration.Schedulings)
            {
                if (!CronExpression.TryParse(schedule.Interval, CronFormat.Standard, out _))
                    throw new BadRequestException("Interval must be a 5 fields valid cron expression (ex.: \"0 2 * * 1\").");
            }
        }
    }
}
