using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using Investager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Investager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly IDataProviderServiceFactory _dataProviderServiceFactory;
        private readonly IDataCollectionServiceFactory _dataCollectionServiceFactory;
        private readonly IAssetService _assetService;

        public AssetController(
            IDataProviderServiceFactory dataProviderServiceFactory,
            IDataCollectionServiceFactory dataCollectionServiceFactory,
            IAssetService assetService)
        {
            _dataProviderServiceFactory = dataProviderServiceFactory;
            _dataCollectionServiceFactory = dataCollectionServiceFactory;
            _assetService = assetService;
        }

        [HttpGet("search/{searchText}")]
        public async Task<IActionResult> Search(string searchText, [FromQuery] int max)
        {
            var response = await _assetService.Search(searchText, max);

            return Ok(response);
        }

        [HttpGet("starred")]
        public async Task<IActionResult> GetStarred()
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            var starredAssets = await _assetService.GetStarred(userId);

            return Ok(starredAssets);
        }

        [HttpPost("scan")]
        public async Task<IActionResult> Scan()
        {
            var assetService = _dataProviderServiceFactory.CreateService(DataProviders.Alpaca);
            await assetService.ScanAssets();

            return NoContent();
        }

        [HttpPost("start")]
        public IActionResult Start()
        {
            var dataCollectionService = _dataCollectionServiceFactory.GetService(DataProviders.Alpaca);
            dataCollectionService.Start();

            return NoContent();
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            var dataCollectionService = _dataCollectionServiceFactory.GetService(DataProviders.Alpaca);
            dataCollectionService.Stop();

            return NoContent();
        }

        [HttpPost("star")]
        public async Task<IActionResult> Star([FromBody] StarAssetRequest request)
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            await _assetService.Star(userId, request);

            return NoContent();
        }

        [HttpPut("stardisplayorder")]
        public async Task<IActionResult> UpdateStarDisplayOrder([FromBody] StarAssetRequest request)
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            await _assetService.UpdateStarDisplayOrder(userId, request);

            return NoContent();
        }

        [HttpDelete("{assetId}/unstar")]
        public async Task<IActionResult> Unstar(int assetId)
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            await _assetService.Unstar(userId, assetId);

            return NoContent();
        }
    }
}
