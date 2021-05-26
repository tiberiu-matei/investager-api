using AutoMapper;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class AssetService : IAssetService
    {
        private const string AssetDtosCacheKey = "AssetDtos";
        private static readonly TimeSpan AssetDtosTtl = TimeSpan.FromMinutes(30);

        private readonly ICoreUnitOfWork _coreUnitOfWork;
        private readonly IMapper _mapper;
        private readonly ICache _cache;

        public AssetService(ICoreUnitOfWork coreUnitOfWork, IMapper mapper, ICache cache)
        {
            _coreUnitOfWork = coreUnitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public Task<IEnumerable<AssetSummaryDto>> GetAll()
        {
            return _cache.Get(
                AssetDtosCacheKey,
                AssetDtosTtl,
                async () =>
                {
                    var assets = await _coreUnitOfWork.Assets.GetAll();
                    return _mapper.Map<IEnumerable<AssetSummaryDto>>(assets);
                });
        }
    }
}
