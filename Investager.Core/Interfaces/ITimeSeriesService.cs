using Investager.Core.Dtos;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface ITimeSeriesService
    {
        Task<TimeSeriesResponse> Get(string key);
    }
}
