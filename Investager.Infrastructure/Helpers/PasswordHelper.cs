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

        public bool IsPasswordValid(string password, EncodedPassword encodedPassword)
        {
            if (encodedPassword.Salt.Length != 128 || encodedPassword.Hash.Length != 64)
            {
                throw new ArgumentException("Invalid length of password salt or hash.");
            }

            using var hmac = new HMACSHA512(encodedPassword.Salt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != encodedPassword.Hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
