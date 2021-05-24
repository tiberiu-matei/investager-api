using AutoMapper;
using Investager.Core.Dtos;
using Investager.Core.Models;
using Investager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly IDataProviderServiceFactory _dataProviderServiceFactory;
        private readonly IDataCollectionServiceFactory _dataCollectionServiceFactory;
        private readonly ICoreUnitOfWork _coreUnitOfWork;
        private readonly IMapper _mapper;

        public AssetController(
            IDataProviderServiceFactory dataProviderServiceFactory,
            IDataCollectionServiceFactory dataCollectionServiceFactory,
            ICoreUnitOfWork coreUnitOfWork,
            IMapper mapper)
        {
            _dataProviderServiceFactory = dataProviderServiceFactory;
            _dataCollectionServiceFactory = dataCollectionServiceFactory;
            _coreUnitOfWork = coreUnitOfWork;
            _mapper = mapper;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _coreUnitOfWork.Assets.GetAll();
            var assetDtos = _mapper.Map<IEnumerable<AssetSummaryDto>>(assets);

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
