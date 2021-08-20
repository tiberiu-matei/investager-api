using Investager.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IAssetService
    {
        Task<AssetSearchResponse> Search(string text, int max);

        Task<IEnumerable<StarredAssetResponse>> GetStarred(int userId);

        Task Star(int userId, StarAssetRequest request);

        Task UpdateStarDisplayOrder(int userId, StarAssetRequest request);

        Task Unstar(int userId, int assetId);
    }
}
