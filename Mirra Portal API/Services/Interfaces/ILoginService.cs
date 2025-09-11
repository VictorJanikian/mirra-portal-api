using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ILoginService
    {
        public Task<(Token token, Customer customer)> Login(string email, string password);
    }
}
