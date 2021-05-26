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

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var assetDtos = await _assetService.GetAll();

            return Ok(assetDtos);
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
    }
}
