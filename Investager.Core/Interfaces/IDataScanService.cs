using Investager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces;

public interface IDataScanService
{
    public Task<IEnumerable<Asset>> GetAssets();
}
