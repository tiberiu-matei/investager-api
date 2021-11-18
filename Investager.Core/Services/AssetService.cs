using AutoMapper;
using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Core.Services;

public class AssetService : IAssetService
{
    private static readonly TimeSpan AssetDtosTtl = TimeSpan.FromDays(1);

    private readonly ICoreUnitOfWork _coreUnitOfWork;
    private readonly IMapper _mapper;
    private readonly ICache _cache;
    private readonly ITimeSeriesService _timeSeriesService;
    private readonly IFuzzyMatch _fuzzyMatch;

    public AssetService(
        ICoreUnitOfWork coreUnitOfWork,
        IMapper mapper,
        ICache cache,
        ITimeSeriesService timeSeriesService,
        IFuzzyMatch fuzzyMatch)
    {
        _coreUnitOfWork = coreUnitOfWork;
        _mapper = mapper;
        _cache = cache;
        _timeSeriesService = timeSeriesService;
        _fuzzyMatch = fuzzyMatch;
    }

    public async Task<AssetSearchResponse> Search(string text, int max)
    {
        var assets = await _cache.Get(
            CacheKeys.AssetDtos,
            AssetDtosTtl,
            async () =>
            {
                var assets = await GetAssets();

                return _mapper.Map<IEnumerable<AssetSummaryDto>>(assets);
            });

        var symbolMatches = new List<(AssetSummaryDto dto, int match)>();
        var nameMatches = new List<(AssetSummaryDto dto, int match)>();

        foreach (var asset in assets)
        {
            symbolMatches.Add((asset, _fuzzyMatch.Compute(text, asset.Symbol)));
            nameMatches.Add((asset, _fuzzyMatch.Compute(text, asset.Name)));
        }

        var orderedSymbolMatches = symbolMatches.Where(e => e.match <= 2).OrderBy(e => e.match);
        var orderedNameMatches = nameMatches.Where(e => e.match <= 2).OrderBy(e => e.match);

        var responseAssets = orderedSymbolMatches.Take(max).Select(e => e.dto).ToList();

        if (orderedSymbolMatches.Count() < max)
        {
            var takenAssetIds = responseAssets.Select(e => e.Id).ToList();

            var nameAssets = orderedNameMatches
                .Where(e => !takenAssetIds.Contains(e.dto.Id))
                .Take(max - orderedSymbolMatches.Count())
                .Select(e => e.dto)
                .ToList();

            responseAssets.AddRange(nameAssets);
        }

        var response = new AssetSearchResponse
        {
            Assets = responseAssets,
        };

        var gainLossTasks = response.Assets.Select(async e =>
        {
            var timeSeriesSummary = await _timeSeriesService.Get(e.Key);
            e.GainLoss = timeSeriesSummary.GainLoss;
        });

        await Task.WhenAll(gainLossTasks);

        if (orderedSymbolMatches.Count() + orderedNameMatches.Count() > max)
        {
            response.MoreRecordsAvailable = true;
        }

        return response;
    }

    public Task<IEnumerable<Asset>> GetAssets()
    {
        return _cache.Get(
            CacheKeys.Assets,
            AssetDtosTtl,
            () => _coreUnitOfWork.Assets.GetAll());
    }
}
