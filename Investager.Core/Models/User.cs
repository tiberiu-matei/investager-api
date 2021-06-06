using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string DisplayEmail { get; set; }

        public string DisplayName { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    }
}
