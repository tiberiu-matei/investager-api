using Investager.Core.Dtos;
using Investager.Core.Models;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface ITimeSeriesService
    {
        Task<TimeSeriesSummary> Get(string key);

        Task<TimeSeriesSummary> Get(CurrencyPair currencyPair);
    }
}
