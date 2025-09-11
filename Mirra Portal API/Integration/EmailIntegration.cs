using Azure;
using Azure.Communication.Email;
using Mirra_Portal_API.Integration.Interfaces;

namespace Mirra_Portal_API.Integration
{
    public class EmailIntegration : IEmailIntegration
    {
        private readonly EmailClient _emailClient;

        public EmailIntegration(IConfiguration configuration)
        {
            var connectionString = configuration["Email:ConnectionString"] ?? configuration["Email.ConnectionString"];
            _emailClient = new EmailClient(connectionString);
        }

        public async Task SendEmail(string sender, string recipient, string subject, string body)
        {
            var emailContent = new EmailContent(subject)
            {
                Html = body
            };

            var emailRecipients = new EmailRecipients(
                new List<EmailAddress>
                {
                    new EmailAddress(recipient)
                }
            );

            var emailMessage = new EmailMessage(sender, emailRecipients, emailContent);

            await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);
        }

    }
}
