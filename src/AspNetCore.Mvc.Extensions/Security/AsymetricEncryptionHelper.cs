using System.IO;
using System.Security.Cryptography;

namespace AspNetCore.Mvc.Extensions.Security
{
    public class AsymmetricEncryptionHelper
    {
        public class RsaWithCspKey
        {
            const string ContainerName = "MyContainer";

            public void AssignNewKey()
            {
                CspParameters cspParams = new CspParameters(1);
                cspParams.KeyContainerName = ContainerName;
                cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
                cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";

                var rsa = new RSACryptoServiceProvider(cspParams) { PersistKeyInCsp = true };
            }

            public void DeleteKeyInCsp()
            {
                var cspParams = new CspParameters { KeyContainerName = ContainerName };
                var rsa = new RSACryptoServiceProvider(cspParams) { PersistKeyInCsp = false };

                rsa.Clear();
            }

            public byte[] EncryptData(byte[] dataToEncrypt)
            {
                byte[] cipherbytes;

                var cspParams = new CspParameters { KeyContainerName = ContainerName };

                using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
                {
                    cipherbytes = rsa.Encrypt(dataToEncrypt, false);
                }

                return cipherbytes;
            }

            public byte[] DecryptData(byte[] dataToDecrypt)
            {
                byte[] plain;

                var cspParams = new CspParameters { KeyContainerName = ContainerName };

                using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
                {
                    plain = rsa.Decrypt(dataToDecrypt, false);
                }

                return plain;
            }

            public static byte[] CreateDigitalSignature(byte[] hashOfDataToSign)
            {
                var cspParams = new CspParameters { KeyContainerName = ContainerName };

                using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
                {

                    var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                    rsaFormatter.SetHashAlgorithm("SHA256");

                    return rsaFormatter.CreateSignature(hashOfDataToSign);
                }
            }

            public static bool VerifySignature(byte[] hashOfDataToSign, byte[] signature)
            {
                var cspParams = new CspParameters { KeyContainerName = ContainerName };

                using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
                {

                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm("SHA256");

                    return rsaDeformatter.VerifySignature(hashOfDataToSign, signature);
                }
            }
        }

        public class RsaWithRsaParameterKey
        {
            private RSAParameters _publicKey;
            private RSAParameters _privateKey;

            public void AssignNewKey()
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    _publicKey = rsa.ExportParameters(false);
                    _privateKey = rsa.ExportParameters(true);
                }
            }

            public byte[] EncryptData(byte[] dataToEncrypt)
            {
                byte[] cipherbytes;

                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.ImportParameters(_publicKey);

                    cipherbytes = rsa.Encrypt(dataToEncrypt, true);
                }

                return cipherbytes;
            }

            public byte[] DecryptData(byte[] dataToEncrypt)
            {
                byte[] plain;

                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;

                    rsa.ImportParameters(_privateKey);
                    plain = rsa.Decrypt(dataToEncrypt, true);
                }

                return plain;
            }

            public byte[] CreateDigitalSignature(byte[] hashOfDataToSign)
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;

                    rsa.ImportParameters(_privateKey);

                    var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                    rsaFormatter.SetHashAlgorithm("SHA256");

                    return rsaFormatter.CreateSignature(hashOfDataToSign);
                }
            }

            public bool VerifySignature(byte[] hashOfDataToSign, byte[] signature)
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.ImportParameters(_publicKey);

                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm("SHA256");

                    return rsaDeformatter.VerifySignature(hashOfDataToSign, signature);
                }
            }
        }

        public class RsaWithXmlKey
        {
            public void AssignNewKey(string publicKeyPath, string privateKeyPath)
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;

                    if (File.Exists(privateKeyPath))
                    {
                        File.Delete(privateKeyPath);
                    }

                    if (File.Exists(publicKeyPath))
                    {
                        File.Delete(publicKeyPath);
                    }

                    var publicKeyfolder = Path.GetDirectoryName(publicKeyPath);
                    var privateKeyfolder = Path.GetDirectoryName(privateKeyPath);

                    if (!Directory.Exists(publicKeyfolder))
                    {
                        Directory.CreateDirectory(publicKeyfolder);
                    }

                    if (!Directory.Exists(privateKeyfolder))
                    {
                        Directory.CreateDirectory(privateKeyfolder);
                    }

                    File.WriteAllText(publicKeyPath, rsa.ToXmlString(false));
                    File.WriteAllText(privateKeyPath, rsa.ToXmlString(true));
                }
            }

            public byte[] EncryptData(string publicKeyPath, byte[] dataToEncrypt)
            {
                byte[] cipherbytes;

                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.FromXmlString(File.ReadAllText(publicKeyPath));

                    cipherbytes = rsa.Encrypt(dataToEncrypt, false);
                }

                return cipherbytes;
            }

            public byte[] DecryptData(string privateKeyPath, byte[] dataToEncrypt)
            {
                byte[] plain;

                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.FromXmlString(File.ReadAllText(privateKeyPath));
                    plain = rsa.Decrypt(dataToEncrypt, false);
                }

                return plain;
            }

            public static byte[] CreateDigitalSignature(string privateKeyPath, byte[] hashOfDataToSign)
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.PersistKeyInCsp = false;

                    rsa.FromXmlString(File.ReadAllText(privateKeyPath));

                    var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                    rsaFormatter.SetHashAlgorithm("SHA256");

                    return rsaFormatter.CreateSignature(hashOfDataToSign);
                }
            }

            public static bool VerifySignature(string publicKeyPath, byte[] hashOfDataToSign, byte[] signature)
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.FromXmlString(File.ReadAllText(publicKeyPath));

                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm("SHA256");

                    return rsaDeformatter.VerifySignature(hashOfDataToSign, signature);
                }
            }
        }

        public static class RsaWithPEMKey
        {
            public static void AssignNewKey(string publicKeyPath, string privateKeyPath)
            {
                RsaPEMHelper.GenerateRsaKeyPairFiles(publicKeyPath, privateKeyPath);
            }

            public static RSAParameters GetPublicKeyRSAParameters(string publicKeyPath)
            {
                using (var rsa = RsaPEMHelper.PublicKeyFromPemFile(publicKeyPath))
                {
                    return rsa.ExportParameters(false);
                }
            }

            public static RSAParameters GetPrivateKeyRSAParameters(string privateKeyPath)
            {
                using (var rsa = RsaPEMHelper.PrivateKeyFromPemFile(privateKeyPath))
                {
                    return rsa.ExportParameters(true);
                }
            }

            public static byte[] EncryptData(string publicKeyPath, byte[] dataToEncrypt)
            {
                byte[] cipherbytes;

                using (var rsa = RsaPEMHelper.PublicKeyFromPemFile(publicKeyPath))
                {

                    cipherbytes = rsa.Encrypt(dataToEncrypt, false);
                }

                return cipherbytes;
            }

            public static byte[] DecryptData(string privateKeyPath, byte[] dataToEncrypt)
            {
                byte[] plain;

                using (var rsa = RsaPEMHelper.PrivateKeyFromPemFile(privateKeyPath))
                {
                    plain = rsa.Decrypt(dataToEncrypt, false);
                }

                return plain;
            }

            public static byte[] CreateDigitalSignature(string privateKeyPath, byte[] hashOfDataToSign)
            {
                using (var rsa = RsaPEMHelper.PrivateKeyFromPemFile(privateKeyPath))
                {

                    var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                    rsaFormatter.SetHashAlgorithm("SHA256");

                    return rsaFormatter.CreateSignature(hashOfDataToSign);
                }
            }

            public static bool VerifySignature(string publicKeyPath, byte[] hashOfDataToSign, byte[] signature)
            {
                using (var rsa = RsaPEMHelper.PublicKeyFromPemFile(publicKeyPath))
                {
                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm("SHA256");

                    return rsaDeformatter.VerifySignature(hashOfDataToSign, signature);
                }
            }
        }

        public static class RsaWithPEMKeyString
        {
            public static (string publicKey, string privateKey) AssignNewKey()
            {
                return RsaPEMHelper.GenerateRsaKeyPair();
            }

            public static RSAParameters GetPublicKeyRSAParameters(string publicKeyString)
            {
                using (var rsa = RsaPEMHelper.PublicKeyFromPem(publicKeyString))
                {
                    return rsa.ExportParameters(false);
                }
            }

            public static RSAParameters GetPrivateKeyRSAParameters(string privateKeyString)
            {
                using (var rsa = RsaPEMHelper.PrivateKeyFromPem(privateKeyString))
                {
                    return rsa.ExportParameters(true);
                }
            }

            public static byte[] EncryptData(string publicKeyString, byte[] dataToEncrypt)
            {
                byte[] cipherbytes;

                using (var rsa = RsaPEMHelper.PublicKeyFromPem(publicKeyString))
                {

                    cipherbytes = rsa.Encrypt(dataToEncrypt, false);
                }

                return cipherbytes;
            }

            public static byte[] DecryptData(string privateKeyString, byte[] dataToEncrypt)
            {
                byte[] plain;

                using (var rsa = RsaPEMHelper.PrivateKeyFromPem(privateKeyString))
                {
                    plain = rsa.Decrypt(dataToEncrypt, false);
                }

                return plain;
            }

            public static byte[] CreateDigitalSignature(string privateKeyString, byte[] hashOfDataToSign)
            {
                using (var rsa = RsaPEMHelper.PrivateKeyFromPem(privateKeyString))
                {

                    var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                    rsaFormatter.SetHashAlgorithm("SHA256");

                    return rsaFormatter.CreateSignature(hashOfDataToSign);
                }
            }

            public static bool VerifySignature(string publicKeyString, byte[] hashOfDataToSign, byte[] signature)
            {
                using (var rsa = RsaPEMHelper.PublicKeyFromPem(publicKeyString))
                {
                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm("SHA256");

                    return rsaDeformatter.VerifySignature(hashOfDataToSign, signature);
                }
            }
        }
    }
}
