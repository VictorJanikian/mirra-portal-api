using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer> Create(Customer customer);
        Task<Customer> GetByEmail(string email);
    }
}
