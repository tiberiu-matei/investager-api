using System;

namespace Investager.Infrastructure.Settings;

public class CoinGeckoSettings
{
    public TimeSpan DataQueryInterval { get; set; } = TimeSpan.FromSeconds(5);
}
