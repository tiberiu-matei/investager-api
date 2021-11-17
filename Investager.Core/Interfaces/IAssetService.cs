using Investager.Core.Dtos;
using Investager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IAssetService
    {
        Task<AssetSearchResponse> Search(string text, int max);

        Task<IEnumerable<Asset>> GetAssets();
    }
}
