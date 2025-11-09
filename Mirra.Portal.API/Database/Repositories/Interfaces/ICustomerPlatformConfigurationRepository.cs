using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ICustomerPlatformConfigurationRepository
    {
        public Task<CustomerPlatformConfiguration> Create(CustomerPlatformConfiguration configuration);

        public Task<CustomerPlatformConfiguration> GetById(int id);
        public Task<List<CustomerPlatformConfiguration>> GetAllForCustomer(int customerId);
    }
}
