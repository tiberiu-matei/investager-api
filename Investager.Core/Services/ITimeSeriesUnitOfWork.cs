using Investager.Core.Models;
using System;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface ITimeSeriesUnitOfWork : IDisposable
    {
        IGenericRepository<AssetPrice> AssetPrices { get; }

        IGenericRepository<CurrencyExchangeRatio> CurrencyExchangeRatios { get; }

        Task SaveChangesAsync();
    }
}
