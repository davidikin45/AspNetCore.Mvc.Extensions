using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;

namespace AspNetCore.Mvc.Extensions.Security
{
    //https://dejanstojanovic.net/aspnet/2018/june/loading-rsa-key-pair-from-pem-files-in-net-core-with-c/
    public static class RsaPEMHelper
    {
        public static void GenerateRsaKeyPairFiles(String publicKeyFilePath, String privateKeyFilePath)
        {
            if (File.Exists(privateKeyFilePath))
            {
                File.Delete(privateKeyFilePath);
            }

            if (File.Exists(publicKeyFilePath))
            {
                File.Delete(publicKeyFilePath);
            }

            var publicKeyfolder = Path.GetDirectoryName(publicKeyFilePath);
            var privateKeyfolder = Path.GetDirectoryName(privateKeyFilePath);

            if (!Directory.Exists(publicKeyfolder))
            {
                Directory.CreateDirectory(publicKeyfolder);
            }

            if (!Directory.Exists(privateKeyfolder))
            {
                Directory.CreateDirectory(privateKeyfolder);
            }

            var keys = GenerateRsaKeyPair();
            File.WriteAllText(publicKeyFilePath, keys.publicKey);
            File.WriteAllText(privateKeyFilePath, keys.privateKey);

        }

        public static (string publicKey, string privateKey) GenerateRsaKeyPair()
        {
            RsaKeyPairGenerator rsaGenerator = new RsaKeyPairGenerator();
            rsaGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            var keyPair = rsaGenerator.GenerateKeyPair();

            string publicKey;
            using (TextWriter publicKeyTextWriter = new StringWriter())
            {

                PemWriter pemWriter = new PemWriter(publicKeyTextWriter);
                pemWriter.WriteObject(keyPair.Public);
                pemWriter.Writer.Flush();

                publicKey = publicKeyTextWriter.ToString();
            }

            string privateKey;
            using (TextWriter privateKeyTextWriter = new StringWriter())
            {

                PemWriter pemWriter = new PemWriter(privateKeyTextWriter);
                pemWriter.WriteObject(keyPair.Private);
                pemWriter.Writer.Flush();
                privateKey = privateKeyTextWriter.ToString();
            }

            return (publicKey: publicKey, privateKey: privateKey);
        }

        public static RSACryptoServiceProvider PrivateKeyFromPemFile(String filePath)
        {
            return PrivateKeyFromPem(File.ReadAllText(filePath));
        }

        public static RSACryptoServiceProvider PrivateKeyFromPem(String privateKeyPem)
        {
            using (TextReader privateKeyTextReader = new StringReader(privateKeyPem))
            {
                AsymmetricCipherKeyPair readKeyPair = (AsymmetricCipherKeyPair)new PemReader(privateKeyTextReader).ReadObject();


                RsaPrivateCrtKeyParameters privateKeyParams = ((RsaPrivateCrtKeyParameters)readKeyPair.Private);
                RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
                RSAParameters parms = new RSAParameters();

                parms.Modulus = privateKeyParams.Modulus.ToByteArrayUnsigned();
                parms.P = privateKeyParams.P.ToByteArrayUnsigned();
                parms.Q = privateKeyParams.Q.ToByteArrayUnsigned();
                parms.DP = privateKeyParams.DP.ToByteArrayUnsigned();
                parms.DQ = privateKeyParams.DQ.ToByteArrayUnsigned();
                parms.InverseQ = privateKeyParams.QInv.ToByteArrayUnsigned();
                parms.D = privateKeyParams.Exponent.ToByteArrayUnsigned();
                parms.Exponent = privateKeyParams.PublicExponent.ToByteArrayUnsigned();

                cryptoServiceProvider.ImportParameters(parms);

                return cryptoServiceProvider;
            }
        }

        public static RSACryptoServiceProvider PublicKeyFromPemFile(String filePath)
        {
            return PublicKeyFromPem(File.ReadAllText(filePath));
        }

        public static RSACryptoServiceProvider PublicKeyFromPem(String publicKeyPem)
        {
            using (TextReader publicKeyTextReader = new StringReader(publicKeyPem))
            {
                RsaKeyParameters publicKeyParam = (RsaKeyParameters)new PemReader(publicKeyTextReader).ReadObject();

                RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
                RSAParameters parms = new RSAParameters();



                parms.Modulus = publicKeyParam.Modulus.ToByteArrayUnsigned();
                parms.Exponent = publicKeyParam.Exponent.ToByteArrayUnsigned();


                cryptoServiceProvider.ImportParameters(parms);

                return cryptoServiceProvider;
            }
        }
    }
}
