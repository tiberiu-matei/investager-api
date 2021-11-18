using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<WatchlistLightResponse>>> GetWatchlists()
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        var watchlists = await _watchlistService.GetForUser(userId);

        return watchlists.ToList();
    }

    [HttpGet("{watchlistId}")]
    public async Task<ActionResult<WatchlistResponse>> GetWatchlist(int watchlistId)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        var watchlist = await _watchlistService.GetById(userId, watchlistId);

        return watchlist;
    }

    [HttpPost("{watchlistId}/asset/{assetId}/watch")]
    public async Task<IActionResult> WatchAsset(int watchlistId, int assetId, [FromBody] DisplayOrderBody body)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
        var request = new WatchAssetRequest
        {
            UserId = userId,
            WatchlistId = watchlistId,
            AssetId = assetId,
            DisplayOrder = body.DisplayOrder,
        };

        await _watchlistService.WatchAsset(request);

        return NoContent();
    }

    [HttpPost("{watchlistId}/currencypair/{currencyPairId}/watch")]
    public async Task<IActionResult> WatchCurrencyPair(int watchlistId, int currencyPairId, [FromBody] DisplayOrderBody body)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
        var request = new WatchCurrencyPairRequest
        {
            UserId = userId,
            WatchlistId = watchlistId,
            CurrencyPairId = currencyPairId,
            DisplayOrder = body.DisplayOrder,
        };

        await _watchlistService.WatchCurrencyPair(request);

        return NoContent();
    }

    [HttpPut("{watchlistId}/displayorder")]
    public async Task<IActionResult> UpdateDisplayOrder(int watchlistId, [FromBody] DisplayOrderBody body)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        await _watchlistService.UpdateDisplayOrder(userId, watchlistId, body.DisplayOrder);

        return NoContent();
    }

    [HttpPut("{watchlistId}/name")]
    public async Task<IActionResult> UpdateName(int watchlistId, [FromBody] UpdateWatchlistNameBody body)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        await _watchlistService.UpdateName(userId, watchlistId, body.Name);

        return NoContent();
    }

    [HttpDelete("{watchlistId}/asset/{assetId}/unwatch")]
    public async Task<IActionResult> UnwatchAsset(int watchlistId, int assetId)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        await _watchlistService.UnwatchAsset(userId, watchlistId, assetId);

        return NoContent();
    }

    [HttpDelete("{watchlistId}/currencypair/{currencyPairId}/unwatch")]
    public async Task<IActionResult> UnwatchCurrencyPair(int watchlistId, int currencyPairId)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        await _watchlistService.UnwatchCurrencyPair(userId, watchlistId, currencyPairId);

        return NoContent();
    }

    [HttpDelete("{watchlistId}")]
    public async Task<IActionResult> Delete(int watchlistId)
    {
        var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);

        await _watchlistService.Delete(userId, watchlistId);

        return NoContent();
    }
}
