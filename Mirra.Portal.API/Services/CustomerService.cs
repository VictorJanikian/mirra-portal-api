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

        public async Task<Customer> RegisterCustomer(Customer newCustomer)
        {
            var existingCustomer = await _customerRepository.GetByEmail(newCustomer.Email);
            ifCustomerExistsAndIsActivatedThrowsException(existingCustomer);
            newCustomer.Password = BCrypt.Net.BCrypt.HashPassword(newCustomer.Password);
            var createdCustomer = customerExistsButIsNotActivated(existingCustomer) ?
                                  await updateCustomerPassword(existingCustomer, newCustomer) :
                                  await createCustomer(newCustomer);
            await sendActivationCodeByEmail(createdCustomer);
            return createdCustomer;
        }

        private void ifCustomerExistsAndIsActivatedThrowsException(Customer existingCustomer)
        {
            if (customerExistsAndIsActivated(existingCustomer))
            {
                throw new BadRequestException($"Customer with email {existingCustomer.Email} already exists.");
            }
        }

        private Boolean customerExistsAndIsActivated(Customer existingCustomer)
        {
            return existingCustomer != null && existingCustomer.IsEmailActivated == true;
        }

        private Boolean customerExistsButIsNotActivated(Customer existingCustomer)
        {
            return existingCustomer != null && existingCustomer.IsEmailActivated == false;
        }

        private async Task<Customer> updateCustomerPassword(Customer existingCustomer, Customer newCustomer)
        {
            existingCustomer.Password = newCustomer.Password;
            return await _customerRepository.Update(existingCustomer);
        }

        private async Task<Customer> createCustomer(Customer newCustomer)
        {
            return await _customerRepository.Create(newCustomer);
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
