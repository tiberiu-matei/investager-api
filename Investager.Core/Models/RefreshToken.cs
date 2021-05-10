using System;

namespace Investager.Core.Models
{
    public class RefreshToken
    {
        public int Id { get; set; } = default!;

        public string EncodedValue { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public DateTime LastUsedAt { get; set; }

        public User User { get; set; } = default!;
    }
}
