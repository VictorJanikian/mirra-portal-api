namespace Mirra_Portal_API.Integration.Interfaces
{
    public interface IEmailIntegration
    {
        public Task SendEmail(string sender, string recipient, string subject, string body);
    }
}
