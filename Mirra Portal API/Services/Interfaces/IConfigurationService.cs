using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IConfigurationService
    {
        public Task<Customer> CreateConfiguration(Customer customer);
    }
}
