using System;

namespace Investager.Core.Dtos;

public class UpdateAssetDataRequest
{
    public string Exchange { get; set; }

    public string Symbol { get; set; }

    public string Key { get; set; }

    public DateTime? LatestPointTime { get; set; }

}
