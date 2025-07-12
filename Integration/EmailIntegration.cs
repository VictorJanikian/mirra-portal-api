using Mirra_Portal_API.Integration.Interfaces;

namespace Mirra_Portal_API.Integration
{
    public class EmailIntegration : IEmailIntegration
    {
        public Task sendEmail(string sender, string recipient, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}
