using Investager.Core.Models;
using Investager.Core.Services;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Persistence
{
    public class TimeSeriesUnitOfWork : ITimeSeriesUnitOfWork
    {
        private readonly InvestagerTimeSeriesContext _context;

        public TimeSeriesUnitOfWork(InvestagerTimeSeriesContext context)
        {
            _context = context;
            AssetPrices = new TimeSeriesGenericRepository<AssetPrice>(context);
            CurrencyExchangeRatios = new TimeSeriesGenericRepository<CurrencyExchangeRatio>(context);
        }

        public IGenericRepository<AssetPrice> AssetPrices { get; }

        public IGenericRepository<CurrencyExchangeRatio> CurrencyExchangeRatios { get; }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
