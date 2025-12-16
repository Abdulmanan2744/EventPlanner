using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Security
{
    public class KeyGenerator
    {
        public static string GenerateKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var key = new byte[32]; // 256 bits / 32 bytes
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }
    }
}
