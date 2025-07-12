using Mirra_Portal_API.Integration.Interfaces;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailIntegration _emailIntegration;
        public EmailService(IEmailIntegration emailIntegration)
        {
            _emailIntegration = emailIntegration;
        }
        public async Task SendActivationCode(string recipientEmail, string code)
        {
            var subject = code + " is your Mirra AI activation code";

        }

    }
}
