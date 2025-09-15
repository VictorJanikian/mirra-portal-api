using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;
using System.Security.Cryptography;

namespace Mirra_Portal_API.Services
{
    public class CustomerService : ICustomerService
    {
        ICustomerRepository _customerRepository;
        IEmailService _emailService;

        public CustomerService(ICustomerRepository customerRepository, IEmailService emailService)
        {
            _customerRepository = customerRepository;
            _emailService = emailService;
        }

        public async Task<Customer> RegisterCustomer(Customer customer)
        {
            await checkIfCustomerIsAlreadyRegistered(customer.Email);
            customer.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);
            var createdCustomer = await _customerRepository.Create(customer);
            await sendActivationCodeByEmail(customer);
            return createdCustomer;
        }

        private async Task checkIfCustomerIsAlreadyRegistered(string email)
        {
            var existingCustomer = await _customerRepository.GetByEmail(email);
            if (existingCustomer != null)
            {
                throw new BadRequestException($"Customer with email {email} already exists.");
            }
        }

        private async Task sendActivationCodeByEmail(Customer customer)
        {
            if (customer.IsEmailActivated)
                return;

            var activationCode = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");
            customer.EmailActivationCode = activationCode;
            customer.EmailActivationFailedAttempts = 0;
            await _customerRepository.Update(customer);
            await _emailService.SendActivationCode(customer.Email, activationCode);
        }
    }
}
