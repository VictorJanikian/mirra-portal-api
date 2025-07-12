using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ICustomerService
    {
        public Task<Customer> RegisterCustomer(Customer customer);
    }
}
