using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CargoThrive.Infrastructure.Helpers
{
    // CargoThrive.Infrastructure/Helpers/PasswordHasher.cs
    public static class PasswordHasher
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = Convert.ToBase64String(hmac.Key);
            var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return (hash, salt);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            try
            {
                var saltBytes = Convert.FromBase64String(storedSalt);
                var hashBytes = Convert.FromBase64String(storedHash);

                using var hmac = new HMACSHA512(saltBytes);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(hashBytes);
            }
            catch
            {
                return false; // 盐值/哈希格式错误时视为验证失败
            }
        }
    }
}
