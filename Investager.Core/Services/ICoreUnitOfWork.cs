using Investager.Core.Models;
using System;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface ICoreUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }

        IGenericRepository<RefreshToken> RefreshTokens { get; }

        IGenericRepository<Asset> Assets { get; }

        IGenericRepository<Currency> Currencies { get; }

        IGenericRepository<CurrencyPair> CurrencyPairs { get; }

        IGenericRepository<Watchlist> Watchlists { get; }

        IGenericRepository<WatchlistAsset> WatchlistAssets { get; }

        IGenericRepository<WatchlistCurrencyPair> WatchlistCurrencyPairs { get; }

        Task<int> SaveChanges();
    }
}
