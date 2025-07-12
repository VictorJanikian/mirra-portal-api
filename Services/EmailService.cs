using Mirra_Portal_API.Integration.Interfaces;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailIntegration _emailIntegration;
        private readonly IConfiguration _configuration;
        public EmailService(IEmailIntegration emailIntegration, IConfiguration configuration)
        {
            _emailIntegration = emailIntegration;
            _configuration = configuration;
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
    }
}
