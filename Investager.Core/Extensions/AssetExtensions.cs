using Investager.Core.Models;

namespace Investager.Core.Extensions;

public static class AssetExtensions
{
    public static string GetKey(this Asset asset)
    {
        return $"{asset.Exchange}:{asset.Symbol}";
    }
}
