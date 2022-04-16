using Investager.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistLightResponse>> GetForUser(int userId);

    Task<WatchlistResponse> GetById(int userId, int watchlistId);

    Task Add(AddWatchlistRequest request);

    Task WatchAsset(WatchAssetRequest request);

    Task WatchCurrencyPair(WatchCurrencyPairRequest request);

    Task UpdateDisplayOrder(int userId, int watchlistId, int displayOrder);

    Task UpdateName(int userId, int watchlistId, string name);

    Task UnwatchAsset(int userId, int watchlistId, int assetId);

    Task UnwatchCurrencyPair(UnwatchCurrencyPairRequest request);

    Task Delete(int userId, int watchlistId);
}
