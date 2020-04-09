using System;
using System.Security.Cryptography;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Security
{
    public class Hmac
	{
        public static string GenerateKeyBase64()
        {
            return Convert.ToBase64String(GeneratePrivateKey());
        }

        public static byte[] GeneratePrivateKey(int keySizeBytes = 32)
		{
			using (var randomNumberGenerator = new RNGCryptoServiceProvider())
			{
				var randomNumber = new byte[keySizeBytes];
				randomNumberGenerator.GetBytes(randomNumber);

				return randomNumber;
			}
		}

        public static string ComputeHashSha256Base58StringWithHMACKey(string toBeHashed, string key)
        {
            var keyToUse = Encoding.UTF8.GetBytes(key);
            var message = Encoding.UTF8.GetBytes(toBeHashed);
            return Base58.Encode(ComputeHmacsha256(message, keyToUse));
        }

        public static string ComputeHashSha256Base64StringWithHMACKey(string toBeHashed, string key)
        {
            var keyToUse = Encoding.UTF8.GetBytes(key);
            var message = Encoding.UTF8.GetBytes(toBeHashed);

            return Convert.ToBase64String(ComputeHmacsha256(message, keyToUse));
        }

        public static byte[] ComputeHmacsha256(byte[] toBeHashed, byte[] key)
		{
			using (var hmac = new HMACSHA256(key))
			{
				return hmac.ComputeHash(toBeHashed);
			}
		}

        public static byte[] ComputeHmacsha1(byte[] toBeHashed, byte[] key)
        {
            using (var hmac = new HMACSHA1(key))
            {
                return hmac.ComputeHash(toBeHashed);
            }
        }

        public static byte[] ComputeHmacsha512(byte[] toBeHashed, byte[] key)
        {
            using (var hmac = new HMACSHA512(key))
            {
                return hmac.ComputeHash(toBeHashed);
            }
        }

        public static byte[] ComputeHmacmd5(byte[] toBeHashed, byte[] key)
        {
            using (var hmac = new HMACMD5(key))
            {
                return hmac.ComputeHash(toBeHashed);
            }
        }
    }
}
