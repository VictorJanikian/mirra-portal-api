using System.Security.Cryptography;
using System.Text;

namespace Mirra_Portal_API.Helper
{
    public class SymmetricEncryptionHelper
    {
        IConfiguration _configuration;

        public SymmetricEncryptionHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Encrypt(string plainText)
        {
            var key = Convert.FromBase64String(_configuration.GetValue<String>("EncryptionKey"));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();                    // IV aleatório
            using var encryptor = aes.CreateEncryptor();

            byte[] cipherBytes;
            using (var ms = new MemoryStream())
            {
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using var sw = new StreamWriter(cs, Encoding.UTF8);
                sw.Write(plainText);
                sw.Close();
                cipherBytes = ms.ToArray();
            }

            // Armazene IV + cipher (IV tem 16 bytes para AES)
            var combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(combined);
        }

        public string Decrypt(string base64)
        {
            var key = Convert.FromBase64String(_configuration.GetValue<String>("EncryptionKey"));

            var combined = Convert.FromBase64String(base64);
            using var aes = Aes.Create();
            aes.Key = key;

            // Extrai IV (primeiros 16 bytes)
            var iv = new byte[aes.BlockSize / 8];
            var cipher = new byte[combined.Length - iv.Length];
            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(combined, iv.Length, cipher, 0, cipher.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            return sr.ReadToEnd();               // senha “em claro”
        }
    }
}
