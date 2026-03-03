using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer> Create(Customer customer);
        Task<Customer> GetByEmail(string email);
        Task<Customer> GetById(int customerId);
        Task<Customer> GetByStripeCustomerId(string stripeCustomerId);
        Task<Customer> Update(Customer customer);
        Task<Customer> GetByConfigurationId(int configurationId);
    }
}
