using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Core.Services;

public class WatchlistService : IWatchlistService
{
    private readonly ICoreUnitOfWork _unitOfWork;
    private readonly ITimeSeriesService _timeSeriesService;

    public WatchlistService(
        ICoreUnitOfWork coreUnitOfWork,
        ITimeSeriesService timeSeriesService)
    {
        _unitOfWork = coreUnitOfWork;
        _timeSeriesService = timeSeriesService;
    }

    public async Task<IEnumerable<WatchlistLightResponse>> GetForUser(int userId)
    {
        var watchlists = await _unitOfWork.Watchlists.Find(e => e.UserId == userId);

        return watchlists.Select(e => new WatchlistLightResponse
        {
            Id = e.Id,
            Name = e.Name,
            DisplayOrder = e.DisplayOrder,
        }).ToList();
    }

    public async Task<WatchlistResponse> GetById(int userId, int watchlistId)
    {
        var queryResponse = await _unitOfWork.Watchlists.Find(
            e => e.UserId == userId && e.Id == watchlistId,
            e => e
                .Include(x => x.Assets)
                    .ThenInclude(x => x.Asset)
                .Include(x => x.CurrencyPairs)
                    .ThenInclude(x => x.CurrencyPair)
                        .ThenInclude(x => x.FirstCurrency)
                .Include(x => x.CurrencyPairs)
                    .ThenInclude(x => x.CurrencyPair)
                        .ThenInclude(x => x.SecondCurrency));

        var watchlist = queryResponse.Single();

        var watchedAssets = new BlockingCollection<WatchedAssetResponse>();
        var watchedCurrencyPairs = new BlockingCollection<WatchedCurrencyPairResponse>();

        var watchedAssetTasks = watchlist.Assets.Select(async (e) =>
        {
            var watchedAssetResponse = await GetWatchedAssetResponse(e);
            watchedAssets.Add(watchedAssetResponse);
        });
        var watchedCurrencyPairTasks = watchlist.CurrencyPairs.Select(async (e) =>
        {
            var watchedCurrencyPairResponse = await GetWatchedCurrencyPairResponse(e);
            watchedCurrencyPairs.Add(watchedCurrencyPairResponse);
        });

        await Task.WhenAll(watchedAssetTasks);
        await Task.WhenAll(watchedCurrencyPairTasks);

        return new WatchlistResponse
        {
            Id = watchlist.Id,
            Assets = watchedAssets.ToList(),
            CurrencyPairs = watchedCurrencyPairs.ToList(),
        };
    }

    public async Task Add(AddWatchlistRequest request)
    {
        var watchlist = new Watchlist
        {
            Name = request.WatchlistName,
            UserId = request.UserId,
        };

        _unitOfWork.Watchlists.Add(watchlist);

        await _unitOfWork.SaveChanges();
    }

    public async Task WatchAsset(WatchAssetRequest request)
    {
        await VerifyWatchlistOwnership(request.UserId, request.WatchlistId);

        var watchlistAsset = new WatchlistAsset
        {
            WatchlistId = request.WatchlistId,
            AssetId = request.AssetId,
            DisplayOrder = request.DisplayOrder,
        };

        _unitOfWork.WatchlistAssets.Add(watchlistAsset);
        await _unitOfWork.SaveChanges();
    }

    public async Task WatchCurrencyPair(WatchCurrencyPairRequest request)
    {
        await VerifyWatchlistOwnership(request.UserId, request.WatchlistId);

        var watchlistCurrencyPair = new WatchlistCurrencyPair
        {
            WatchlistId = request.WatchlistId,
            FirstCurrencyId = request.FirstCurrencyId,
            SecondCurrencyId = request.SecondCurrencyId,
            DisplayOrder = request.DisplayOrder,
        };

        _unitOfWork.WatchlistCurrencyPairs.Add(watchlistCurrencyPair);
        await _unitOfWork.SaveChanges();
    }

    public async Task UpdateDisplayOrder(int userId, int watchlistId, int displayOrder)
    {
        var watchlistQuery = await _unitOfWork.Watchlists
            .FindWithTracking(e => e.Id == watchlistId && e.UserId == userId);

        var watchlist = watchlistQuery.Single();
        watchlist.DisplayOrder = displayOrder;

        await _unitOfWork.SaveChanges();
    }

    public async Task UpdateName(int userId, int watchlistId, string name)
    {
        var watchlistQuery = await _unitOfWork.Watchlists
            .FindWithTracking(e => e.Id == watchlistId && e.UserId == userId);

        var watchlist = watchlistQuery.Single();
        watchlist.Name = name;

        await _unitOfWork.SaveChanges();
    }

    public async Task UnwatchAsset(int userId, int watchlistId, int assetId)
    {
        await VerifyWatchlistOwnership(userId, watchlistId);

        _unitOfWork.WatchlistAssets
            .Delete(e => e.WatchlistId == watchlistId && e.AssetId == assetId);

        await _unitOfWork.SaveChanges();
    }

    public async Task UnwatchCurrencyPair(UnwatchCurrencyPairRequest request)
    {
        await VerifyWatchlistOwnership(request.UserId, request.WatchlistId);

        _unitOfWork.WatchlistCurrencyPairs
            .Delete(e => e.WatchlistId == request.WatchlistId
                && e.FirstCurrencyId == request.FirstCurrencyId
                && e.SecondCurrencyId == request.SecondCurrencyId);

        await _unitOfWork.SaveChanges();
    }

    public async Task Delete(int userId, int watchlistId)
    {
        _unitOfWork.Watchlists
            .Delete(e => e.Id == watchlistId && e.UserId == userId);

        await _unitOfWork.SaveChanges();
    }

    private async Task<WatchedAssetResponse> GetWatchedAssetResponse(WatchlistAsset watchlistAsset)
    {
        var key = $"{watchlistAsset.Asset.Exchange}:{watchlistAsset.Asset.Symbol}";

        var timeSeriesSummary = await _timeSeriesService.Get(key);

        var response = new WatchedAssetResponse
        {
            AssetId = watchlistAsset.AssetId,
            Symbol = watchlistAsset.Asset.Symbol,
            Exchange = watchlistAsset.Asset.Exchange,
            Key = key,
            Name = watchlistAsset.Asset.Name,
            Industry = watchlistAsset.Asset.Industry,
            Currency = watchlistAsset.Asset.Currency,
            DisplayOrder = watchlistAsset.DisplayOrder,
            GainLoss = timeSeriesSummary.GainLoss,
        };

        return response;
    }

    private async Task<WatchedCurrencyPairResponse> GetWatchedCurrencyPairResponse(WatchlistCurrencyPair watchlistCurrencyPair)
    {
        var timeSeriesSummary = await _timeSeriesService.Get(watchlistCurrencyPair.CurrencyPair);

        var response = new WatchedCurrencyPairResponse
        {
            FirstCurrencyName = watchlistCurrencyPair.CurrencyPair.FirstCurrency.Name,
            SecondCurrencyName = watchlistCurrencyPair.CurrencyPair.SecondCurrency.Name,
            Key = timeSeriesSummary.Key,
            DisplayOrder = watchlistCurrencyPair.DisplayOrder,
            GainLoss = timeSeriesSummary.GainLoss,
        };

        return response;
    }

    private async Task VerifyWatchlistOwnership(int userId, int watchlistId)
    {
        var watchlistQuery = await _unitOfWork
            .Watchlists
            .Find(e => e.Id == watchlistId && e.UserId == userId);

        var verifyOwnership = watchlistQuery.Single();
    }
}
