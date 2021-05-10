using Investager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Investager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly IDataProviderServiceFactory _assetServiceFactory;
        private readonly IDataCollectionServiceFactory _dataCollectionServiceFactory;

        public AssetController(IDataProviderServiceFactory assetServiceFactory, IDataCollectionServiceFactory dataCollectionServiceFactory)
        {
            _assetServiceFactory = assetServiceFactory;
            _dataCollectionServiceFactory = dataCollectionServiceFactory;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> Scan()
        {
            try
            {
                var assetService = _assetServiceFactory.CreateService("Alpaca");
                await assetService.ScanAssetsAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("start")]
        public IActionResult Start()
        {
            try
            {
                var dataCollectionService = _dataCollectionServiceFactory.GetService("Alpaca");
                dataCollectionService.Start();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            try
            {
                var dataCollectionService = _dataCollectionServiceFactory.GetService("Alpaca");
                dataCollectionService.Stop();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
