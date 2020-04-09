using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Security
{
    //https://github.com/jrnker/CSharp-easy-RSA-PEM
    public class SymmetricEncryptionHelper
    {
        //Best Practice
        //iv is a salt and should be 16 bytes
        public class AesEncryption
        {
 


            public static byte[] GenerateRandomNumber(int length = 32)
            {
                using (var randomNumberGenerator = new RNGCryptoServiceProvider())
                {
                    var randomNumber = new byte[length];
                    randomNumberGenerator.GetBytes(randomNumber);

                    return randomNumber;
                }
            }

            public static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
            {
                return Encrypt(Encoding.UTF8.GetBytes(plainText), key, iv);
            }

            public static byte[] Encrypt(byte[] dataToEncrypt, byte[] key, byte[] iv)
            {
                using (var aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    aes.Key = key;
                    aes.IV = iv;

                    using (var memoryStream = new MemoryStream())
                    {
                        var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(),
                            CryptoStreamMode.Write);

                        cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                        cryptoStream.FlushFinalBlock();

                        return memoryStream.ToArray();
                    }
                }
            }

            public static byte[] Decrypt(byte[] dataToDecrypt, byte[] key, byte[] iv)
            {
                using (var aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    aes.Key = key;
                    aes.IV = iv;

                    using (var memoryStream = new MemoryStream())
                    {
                        var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(),
                            CryptoStreamMode.Write);

                        cryptoStream.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                        cryptoStream.FlushFinalBlock();

                        var decryptBytes = memoryStream.ToArray();

                        return decryptBytes;
                    }
                }
            }
        }

        public class DesEncryption
        {
            public byte[] GenerateRandomNumber(int length = 32)
            {
                using (var randomNumberGenerator = new RNGCryptoServiceProvider())
                {
                    var randomNumber = new byte[length];
                    randomNumberGenerator.GetBytes(randomNumber);

                    return randomNumber;
                }
            }

            public byte[] Encrypt(byte[] dataToEncrypt, byte[] key, byte[] iv)
            {
                using (var des = new DESCryptoServiceProvider())
                {
                    des.Mode = CipherMode.CBC;
                    des.Padding = PaddingMode.PKCS7;

                    des.Key = key;
                    des.IV = iv;

                    using (var memoryStream = new MemoryStream())
                    {
                        var cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(),
                            CryptoStreamMode.Write);

                        cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                        cryptoStream.FlushFinalBlock();

                        return memoryStream.ToArray();
                    }
                }
            }

            public byte[] Decrypt(byte[] dataToDecrypt, byte[] key, byte[] iv)
            {
                using (var des = new DESCryptoServiceProvider())
                {
                    des.Mode = CipherMode.CBC;
                    des.Padding = PaddingMode.PKCS7;

                    des.Key = key;
                    des.IV = iv;

                    using (var memoryStream = new MemoryStream())
                    {
                        var cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(),
                            CryptoStreamMode.Write);

                        cryptoStream.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                        cryptoStream.FlushFinalBlock();

                        return memoryStream.ToArray();
                    }
                }
            }
        }

        public class TripleDesEncryption
        {
            public byte[] GenerateRandomNumber(int length = 32)
            {
                using (var randomNumberGenerator = new RNGCryptoServiceProvider())
                {
                    var randomNumber = new byte[length];
                    randomNumberGenerator.GetBytes(randomNumber);

                    return randomNumber;
                }
            }

            public byte[] Encrypt(byte[] dataToEncrypt, byte[] key, byte[] iv)
            {
                using (var des = new TripleDESCryptoServiceProvider())
                {
                    des.Mode = CipherMode.CBC;
                    des.Padding = PaddingMode.PKCS7;

                    des.Key = key;
                    des.IV = iv;

                    using (var memoryStream = new MemoryStream())
                    {
                        var cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(),
                            CryptoStreamMode.Write);

                        cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                        cryptoStream.FlushFinalBlock();

                        return memoryStream.ToArray();
                    }
                }
            }

            public byte[] Decrypt(byte[] dataToDecrypt, byte[] key, byte[] iv)
            {
                using (var des = new TripleDESCryptoServiceProvider())
                {
                    des.Mode = CipherMode.CBC;
                    des.Padding = PaddingMode.PKCS7;

                    des.Key = key;
                    des.IV = iv;

                    using (var memoryStream = new MemoryStream())
                    {
                        var cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(),
                            CryptoStreamMode.Write);

                        cryptoStream.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                        cryptoStream.FlushFinalBlock();

                        var decryptBytes = memoryStream.ToArray();

                        return decryptBytes;
                    }
                }
            }
        }
    }
}
