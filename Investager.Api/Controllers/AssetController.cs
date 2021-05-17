﻿using AutoMapper;
using Investager.Core.Dtos;
using Investager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly IDataProviderServiceFactory _assetServiceFactory;
        private readonly IDataCollectionServiceFactory _dataCollectionServiceFactory;
        private readonly ICoreUnitOfWork _coreUnitOfWork;
        private readonly IMapper _mapper;

        public AssetController(
            IDataProviderServiceFactory assetServiceFactory,
            IDataCollectionServiceFactory dataCollectionServiceFactory,
            ICoreUnitOfWork coreUnitOfWork,
            IMapper mapper)
        {
            _assetServiceFactory = assetServiceFactory;
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
            try
            {
                var assetService = _assetServiceFactory.CreateService("Alpaca");
                await assetService.ScanAssets();

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
