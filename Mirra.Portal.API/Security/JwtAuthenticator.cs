using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Mirra_Portal_API.Security
{
    public static class JwtAuthenticator
    {
        public static IServiceCollection AddJwtSecurity(this IServiceCollection services, TokenConfigurations tokenConfigurations, string key)
        {
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = getSigningKey(key);
                paramsValidation.ValidAudience = tokenConfigurations.Audience;
                paramsValidation.ValidIssuer = tokenConfigurations.Issuer;
                paramsValidation.ValidateIssuerSigningKey = true;
                paramsValidation.ValidateLifetime = true;
                paramsValidation.ValidateIssuer = true;
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            return services;
        }

        private static SecurityKey getSigningKey(string key)
        {

            var keyBytes = Convert.FromBase64String(key);

            var rsaParameters = new RSAParameters
            {
                Modulus = keyBytes,
                Exponent = new byte[] { 1, 0, 1 }
            };

            return new RsaSecurityKey(rsaParameters);
        }
    }
}
