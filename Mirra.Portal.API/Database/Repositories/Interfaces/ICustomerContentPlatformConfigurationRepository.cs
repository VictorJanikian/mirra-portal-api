using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ICustomerContentPlatformConfigurationRepository
    {
        public Task<CustomerContentPlatformConfiguration> Create(CustomerContentPlatformConfiguration configuration);

        public Task<CustomerContentPlatformConfiguration> GetById(int id);
    }
}
