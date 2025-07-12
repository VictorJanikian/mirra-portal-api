namespace Mirra_Portal_API.Integration.Interfaces
{
    public interface IEmailIntegration
    {
        public Task sendEmail(string sender, string recipient, string subject, string body);
    }
}
