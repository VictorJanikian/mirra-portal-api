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

        public async Task<Customer> GetCustomerById(int customerId)
        {
            var customer = await _customerRepository.GetById(customerId);
            if (customer == null)
                throw new NotFoundException($"Customer with id {customerId} not found.");
            return customer;
        }

        public async Task ForgotPassword(string email)
        {
            var customer = await _customerRepository.GetByEmail(email);
            if (customer == null)
                throw new NotFoundException($"Customer with email {email} not found.");

            if (!customer.IsEmailActivated)
                throw new BadRequestException("E-mail not activated.");

            var resetCode = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");
            customer.PasswordResetCode = resetCode;
            customer.PasswordResetCodeExpiration = DateTime.Now.AddMinutes(15);
            customer.PasswordResetFailedAttempts = 0;
            await _customerRepository.Update(customer);
            await _emailService.SendPasswordResetCode(customer.Email, resetCode);
        }

        public async Task ResetPassword(string email, string code, string newPassword)
        {
            var customer = await _customerRepository.GetByEmail(email);
            if (customer == null)
                throw new NotFoundException($"Customer with email {email} not found.");

            if (string.IsNullOrEmpty(customer.PasswordResetCode))
                throw new BadRequestException("No password reset was requested.");

            if (customer.PasswordResetFailedAttempts >= 5)
                throw new BadRequestException("Too many failed attempts. Please request a new code.");

            if (DateTime.Now > customer.PasswordResetCodeExpiration)
                throw new BadRequestException("Password reset code has expired. Please request a new one.");

            if (code != customer.PasswordResetCode)
            {
                customer.PasswordResetFailedAttempts++;
                await _customerRepository.Update(customer);
                throw new BadRequestException("Invalid reset code. Please try again.");
            }

            customer.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            customer.PasswordResetCode = null;
            customer.PasswordResetCodeExpiration = null;
            customer.PasswordResetFailedAttempts = null;
            await _customerRepository.Update(customer);
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
