using Investager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IDataProviderService
    {
        public Task<IEnumerable<Asset>> ScanAssets();

        public Task UpdateTimeSeriesData(Asset asset);
    }
}
