using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Mirra_Portal_API.Security
{
    public class SigningConfigurations
    {
        public SecurityKey Key { get; }
        public SigningCredentials SigningCredentials { get; }
        public SigningConfigurations(string privateKey)
        {
            var provider = new RSACryptoServiceProvider();
            provider.FromXmlString(privateKey);
            Key = new RsaSecurityKey(provider.ExportParameters(true));
            SigningCredentials = new SigningCredentials(
                Key, SecurityAlgorithms.RsaSha256Signature);
        }
    }
}
