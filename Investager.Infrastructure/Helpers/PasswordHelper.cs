using Investager.Core.Interfaces;
using Investager.Core.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Investager.Infrastructure.Helpers
{
    public class PasswordHelper : IPasswordHelper
    {
        public EncodedPassword EncodePassword(string password)
        {
            using var hmac = new HMACSHA512();
            var passwordSalt = hmac.Key;
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return new EncodedPassword
            {
                Salt = passwordSalt,
                Hash = passwordHash,
            };
        }

        public bool IsPasswordCorrect(string password, byte[] hash, byte[] salt)
        {
            if (hash.Length != 64 || salt.Length != 128)
            {
                throw new ArgumentException("Invalid length of password salt or hash.");
            }

            using var hmac = new HMACSHA512(salt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
