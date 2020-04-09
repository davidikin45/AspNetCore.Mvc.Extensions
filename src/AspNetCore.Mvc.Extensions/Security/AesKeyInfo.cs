using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Security
{
    public class AesKeyInfo
    {
        public byte[] Key { get; }
        public byte[] Iv { get; }

        public string KeyString => Convert.ToBase64String(Key);
        public string IVString => Convert.ToBase64String(Iv);

        public AesKeyInfo()
        {
            using (var myAes = Aes.Create())
            {
                Key = myAes.Key;
                Iv = myAes.IV;
            }
        }

        public AesKeyInfo(string key, string iv)
        {
            Key = Convert.FromBase64String(key);
            Iv = Convert.FromBase64String(iv);
        }

        public AesKeyInfo(byte[] key, byte[] iv)
        {
            Key = key;
            Iv = iv;
        }
    }
}
