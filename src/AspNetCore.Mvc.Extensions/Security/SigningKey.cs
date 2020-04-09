using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Security
{
    public static class SigningKey
    {
        //No kid or x5t
        public static SymmetricSecurityKey LoadSymmetricSecurityKey(string bearerTokenKey, string kidPrefix = "")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenKey));
            key.KeyId = $"{kidPrefix}{HashData.ComputeHashSha256Base64String(bearerTokenKey)}";

            return key;
        }

        //No kid or x5t
        public static RsaSecurityKey LoadPrivateRsaSigningKey(string privateKeyPath, string kidPrefix = "")
        {
            var rsaParameters = AsymmetricEncryptionHelper.RsaWithPEMKey.GetPrivateKeyRSAParameters(privateKeyPath);
            var key = new RsaSecurityKey(rsaParameters);
            key.KeyId = $"{kidPrefix}{HashData.ComputeHashSha256Base64String(Path.GetDirectoryName(privateKeyPath))}";

            return key;
        }

        //No kid or x5t
        public static RsaSecurityKey LoadPublicRsaSigningKey(string publicKeyPath, string kidPrefix = "")
        {
            var rsaParameters = AsymmetricEncryptionHelper.RsaWithPEMKey.GetPublicKeyRSAParameters(publicKeyPath);
            var key = new RsaSecurityKey(rsaParameters);
            key.KeyId = $"{kidPrefix}{HashData.ComputeHashSha256Base64String(Path.GetDirectoryName(publicKeyPath))}";


            return key;
        }

        //kid and x5t auto populated
        //.cer
        public static X509SecurityKey LoadPrivateSigningCertificate(string privateSigningCertificatePath, string password, string kidPrefix = "", string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            X509Certificate2 privateCertificate = new X509Certificate2(privateSigningCertificatePath, password, X509KeyStorageFlags.PersistKeySet);

            var key= new X509SecurityKey(privateCertificate);

            // add signing algorithm name to key ID to allow using the same key for two different algorithms (e.g. RS256 and PS56)
            key.KeyId = $"{kidPrefix}{key.KeyId}{signingAlgorithm}"; //X509Certificate2.Thumbprint

            return key;
        }

        //kid and x5t auto populated
        //.pfx
        public static X509SecurityKey LoadPublicSigningCertificate(string publicSigningCertificatePath, string kidPrefix = "", string signingAlgorithm = SecurityAlgorithms.RsaSha256)
        {
            var publicCertificate = new X509Certificate2(publicSigningCertificatePath);

            var key = new X509SecurityKey(publicCertificate);

            // add signing algorithm name to key ID to allow using the same key for two different algorithms (e.g. RS256 and PS56)
            key.KeyId = $"{kidPrefix}{key.KeyId}{signingAlgorithm}"; //X509Certificate2.Thumbprint

            return key;
        }
    }
}
