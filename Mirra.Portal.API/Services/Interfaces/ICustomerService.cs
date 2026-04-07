using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ICustomerService
    {
        public Task<Customer> RegisterCustomer(Customer customer);
        public Task<Customer> GetCustomerById(int customerId);
        public Task ForgotPassword(string email);
        public Task<(Token token, Customer customer)> ResetPassword(string email, string code, string newPassword);
    }
}
