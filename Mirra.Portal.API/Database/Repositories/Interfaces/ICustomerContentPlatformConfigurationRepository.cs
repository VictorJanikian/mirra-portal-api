using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ICustomerContentPlatformConfigurationRepository
    {
        public Task<CustomerPlatformConfiguration> Create(CustomerPlatformConfiguration configuration);
    }
}
