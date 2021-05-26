using Investager.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IAssetService
    {
        Task<IEnumerable<AssetSummaryDto>> GetAll();
    }
}
