using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Integration.Interfaces;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailIntegration _emailIntegration;
        private readonly IConfiguration _configuration;
        private readonly ICustomerRepository _customerRepository;

        public EmailService(IEmailIntegration emailIntegration, IConfiguration configuration,
            ICustomerRepository customerRepository)
        {
            _emailIntegration = emailIntegration;
            _configuration = configuration;
            _customerRepository = customerRepository;
        }

        public async Task SendActivationCode(string recipientEmail, string code)
        {
            var subject = code + " is your Mirra AI activation code";
            var body = $@"<html><body style='font-family:sans-serif;'>
                <h2>Welcome to Mirra AI!</h2>
                <p>Your activation code is:</p>
                <div style='font-size:2em; font-weight:bold; margin:20px 0; color:#4F46E5;'>{code}</div>
                <p>Enter this code to activate your account. If you did not request this, please ignore this email.</p>
                <br/>
                <p style='color:#888;'>Mirra AI Team</p>
                </body></html>";
            var sender = _configuration["Email:From"];
            await _emailIntegration.SendEmail(sender, recipientEmail, subject, body);
        }

        public async Task ActivateEmail(string email, string code)
        {
            var customer = await _customerRepository.GetByEmail(email);
            if (customer == null)
                throw new NotFoundException($"Customer with email {email} not found.");

            if (customer.IsEmailActivated)
                return;

            if (customer.EmailActivationFailedAttempts >= 5)
                throw new BadRequestException("Too many failed attempts to activate email. Please contact support.");

            if (code == customer.EmailActivationCode)
            {
                await activateEmail(customer);
            }
            else
            {
                await updateFailedAttempts(customer);
                throwInvalidCodeException();
            }

        }

        private static void throwInvalidCodeException()
        {
            throw new BadRequestException("Invalid activation code. Please try again.");
        }

        private async Task activateEmail(Model.Customer customer)
        {
            customer.IsEmailActivated = true;
            await _customerRepository.Update(customer);
        }
        private async Task updateFailedAttempts(Model.Customer customer)
        {
            customer.EmailActivationFailedAttempts++;
            await _customerRepository.Update(customer);
        }

    }
}
