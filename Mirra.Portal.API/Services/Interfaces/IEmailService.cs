using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendActivationCode(string recipientEmail, string code);
        public Task<(Token token, Customer customer)> ActivateEmail(string email, string code);
    }
}
