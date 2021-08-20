using Investager.Core.Dtos;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface ITimeSeriesService
    {
        Task<TimeSeriesSummary> Get(string key);
    }
}
