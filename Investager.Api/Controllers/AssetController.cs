using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Investager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AssetController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetController(
        IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet("search/{searchText}")]
    public async Task<ActionResult<AssetSearchResponse>> Search(string searchText, [FromQuery] int max)
    {
        var response = await _assetService.Search(searchText, max);

        return response;
    }
}
