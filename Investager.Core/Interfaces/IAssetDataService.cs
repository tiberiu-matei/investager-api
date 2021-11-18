using Investager.Core.Dtos;
using Investager.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces;

public interface IAssetDataService
{
    string Provider { get; }

    TimeSpan DataQueryInterval { get; }

    Task<IEnumerable<Asset>> GetAssets();

    Task<IEnumerable<TimeSeriesPoint>> GetRecentPoints(UpdateAssetDataRequest request);
}
