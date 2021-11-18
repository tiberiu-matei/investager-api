using System.Collections.Generic;

namespace Investager.Core.Dtos;

public class AssetSearchResponse
{
    public IEnumerable<AssetSummaryDto> Assets { get; set; }

    public bool MoreRecordsAvailable { get; set; }
}
