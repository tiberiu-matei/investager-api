using System;

namespace Investager.Infrastructure.Settings;

public class DataUpdateSettings
{
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromHours(1);
}
