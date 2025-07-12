using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class CustomerService : ICustomerService
    {
        ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> RegisterCustomer(Customer customer)
        {
            await checkIfCustomerIsAlreadyRegistered(customer.Email);
            customer.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);
            return await _customerRepository.Create(customer);
        }

        private async Task checkIfCustomerIsAlreadyRegistered(string email)
        {
            var existingCustomer = await _customerRepository.GetByEmail(email);
            if (existingCustomer != null)
            {
                throw new BadRequestException($"Customer with email {email} already exists.");
            }
        }
    }
}
