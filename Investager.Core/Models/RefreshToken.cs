using System;

namespace Investager.Core.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public string EncodedValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUsedAt { get; set; }

    public User User { get; set; }

    public int UserId { get; set; }
}
