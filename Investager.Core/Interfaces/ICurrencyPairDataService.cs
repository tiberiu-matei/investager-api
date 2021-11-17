using Investager.Core.Dtos;
using Investager.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface ICurrencyPairDataService
    {
        string Provider { get; }

        TimeSpan DataQueryInterval { get; }

        Task<IEnumerable<CurrencyPair>> GetPairs();

        Task<IEnumerable<TimeSeriesPoint>> GetRecentPoints(UpdateCurrencyPairDataRequest request);
    }
}
