using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Helper;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class ConfigurationService : IConfigurationService
    {
        ICustomerContentPlatformConfigurationRepository _configurationRepository;
        IdentityHelper _identityHelper;

        public ConfigurationService(ICustomerContentPlatformConfigurationRepository configurationRepository,
                                    IdentityHelper identityHelper)
        {
            _configurationRepository = configurationRepository;
            _identityHelper = identityHelper;
        }

        public async Task<CustomerPlatformConfiguration> CreateConfiguration(CustomerPlatformConfiguration configuration)
        {
            configuration.Customer = new Customer { Id = _identityHelper.UserId() };
            return await _configurationRepository.Create(configuration);
        }
    }
}
