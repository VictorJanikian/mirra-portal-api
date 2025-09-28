using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Services.Interfaces
{
    public interface ITokenService
    {
        public Token GenerateToken(Customer customer);
    }
}
