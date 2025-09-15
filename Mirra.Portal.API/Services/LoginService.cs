using Microsoft.IdentityModel.Tokens;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Exceptions;
using Mirra_Portal_API.Model;
using Mirra_Portal_API.Security;
using Mirra_Portal_API.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mirra_Portal_API.Services
{
    public class LoginService : ILoginService
    {
        ICustomerRepository _customerRepository;
        SigningConfigurations _signingConfigurations;

        public LoginService(ICustomerRepository customerRepository, SigningConfigurations signingConfigurations)
        {
            _customerRepository = customerRepository;
            _signingConfigurations = signingConfigurations;
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

            var token = generateToken(customer);
            return (token, customer);
        }

        private Token generateToken(Customer customer)
        {
            ClaimsIdentity identity;
            DateTime createdDate = DateTime.Now;


            identity = new ClaimsIdentity(new Claim[]
                {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(JwtRegisteredClaimNames.Nickname, customer.Name),
                new(JwtRegisteredClaimNames.Email, customer.Email),
                new(ClaimTypes.Sid, customer.Id.ToString()),
             });

            DateTime expireDate = createdDate + TimeSpan.FromSeconds(86400);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = "Mirra AI",
                Audience = "Web Apps",
                SigningCredentials = _signingConfigurations.SigningCredentials,
                Subject = identity,
                NotBefore = createdDate,
                Expires = expireDate,
                IssuedAt = createdDate
            });
            var writeToken = handler.WriteToken(securityToken);

            var token = new Token()
            {
                Value = writeToken,
                DateCreated = createdDate,
                DateExpiration = expireDate
            };

            return token;
        }
    }
}
