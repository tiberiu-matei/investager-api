using Investager.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IAssetService
    {
        Task<IEnumerable<AssetSummaryDto>> GetAll();

        Task<IEnumerable<StarredAssetResponse>> GetStarred(int userId);

        Task Star(int userId, StarAssetRequest request);

        Task UpdateStarDisplayOrder(int userId, StarAssetRequest request);

        Task Unstar(int userId, int assetId);
    }
}
