using AutoMapper;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class AssetService : IAssetService
    {
        private const string AssetDtosCacheKey = "AssetDtos";
        private static readonly TimeSpan AssetDtosTtl = TimeSpan.FromMinutes(1);

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

        public async Task<IEnumerable<StarredAssetResponse>> GetStarred(int userId)
        {
            var starred = await _coreUnitOfWork.UserStarredAssets.Find(e => e.UserId == userId);

            var response = starred.Select(e => new StarredAssetResponse { AssetId = e.AssetId, DisplayOrder = e.DisplayOrder }).ToList();

            return response.OrderBy(e => e.DisplayOrder);
        }

        public async Task Star(int userId, StarAssetRequest request)
        {
            var userStarredAsset = new UserStarredAsset
            {
                UserId = userId,
                AssetId = request.AssetId,
                DisplayOrder = request.DisplayOrder,
            };

            _coreUnitOfWork.UserStarredAssets.Insert(userStarredAsset);
            await _coreUnitOfWork.SaveChanges();
        }

        public async Task UpdateStarDisplayOrder(int userId, StarAssetRequest request)
        {
            var userStarredAssets = await _coreUnitOfWork.UserStarredAssets.FindWithTracking(e => e.UserId == userId && e.AssetId == request.AssetId);
            var userStarredAsset = userStarredAssets.Single();
            userStarredAsset.DisplayOrder = request.DisplayOrder;

            await _coreUnitOfWork.SaveChanges();
        }

        public async Task Unstar(int userId, int assetId)
        {
            var userStarredAssets = await _coreUnitOfWork.UserStarredAssets
                .Find(e => e.UserId == userId && e.AssetId == assetId);
            var userStarredAsset = userStarredAssets.Single();

            _coreUnitOfWork.UserStarredAssets.Delete(userStarredAsset);
            await _coreUnitOfWork.SaveChanges();
        }
    }
}
