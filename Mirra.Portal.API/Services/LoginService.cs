using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Services.Interfaces;

namespace Mirra_Portal_API.Services
{
    public class LoginService : ILoginService
    {
        ICustomerRepository _customerRepository;
        ITokenService _tokenService;

        public LoginService(ICustomerRepository customerRepository, ITokenService tokenService)
        {
            _customerRepository = customerRepository;
            _tokenService = tokenService;
        }

        public async Task<(Token token, Customer customer)> Login(string email, string password)
        {
            var customer = await _customerRepository.GetByEmail(email);

            if (customer == null)
                throw new UnauthorizedException("Invalid email or password.");

            var authenticated = BCrypt.Net.BCrypt.Verify(password, customer.Password);

            if (!authenticated)
                throw new UnauthorizedException("Invalid email or password.");

            if (!customer.IsEmailActivated)
                throw new UnauthorizedException("E-mail not activated.");

            var token = _tokenService.GenerateToken(customer);
            return (token, customer);
        }

    }
}
