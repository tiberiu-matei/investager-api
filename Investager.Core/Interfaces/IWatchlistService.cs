using Investager.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistLightResponse>> GetForUser(int userId);

    Task<WatchlistResponse> GetById(int userId, int watchlistId);

    Task Add(AddWatchlistRequest addWatchlistRequest);

    Task WatchAsset(WatchAssetRequest watchAssetRequest);

    Task WatchCurrencyPair(WatchCurrencyPairRequest watchCurrencyPairRequest);

    Task UpdateDisplayOrder(int userId, int watchlistId, int displayOrder);

    Task UpdateName(int userId, int watchlistId, string name);

    Task UnwatchAsset(int userId, int watchlistId, int assetId);

    Task UnwatchCurrencyPair(int userId, int watchlistId, int currencyPairId);

    Task Delete(int userId, int watchlistId);
}
