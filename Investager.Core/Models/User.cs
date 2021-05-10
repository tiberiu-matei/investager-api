using System.Collections.Generic;

namespace Investager.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; } = default!;

        public string DisplayEmail { get; set; } = default!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public byte[] PasswordHash { get; set; } = default!;

        public byte[] PasswordSalt { get; set; } = default!;

        public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    }
}
