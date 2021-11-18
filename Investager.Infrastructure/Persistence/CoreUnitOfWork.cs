using Investager.Core.Models;
using Investager.Core.Services;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Persistence;

public class CoreUnitOfWork : ICoreUnitOfWork
{
    private readonly InvestagerCoreContext _context;

    public CoreUnitOfWork(InvestagerCoreContext context)
    {
        _context = context;

        Users = new CoreGenericRepository<User>(context);
        RefreshTokens = new CoreGenericRepository<RefreshToken>(context);
        Assets = new CoreGenericRepository<Asset>(context);
        Currencies = new CoreGenericRepository<Currency>(context);
        CurrencyPairs = new CoreGenericRepository<CurrencyPair>(context);
        Watchlists = new CoreGenericRepository<Watchlist>(context);
        WatchlistAssets = new CoreGenericRepository<WatchlistAsset>(context);
        WatchlistCurrencyPairs = new CoreGenericRepository<WatchlistCurrencyPair>(context);
    }

    public IGenericRepository<User> Users { get; }

    public IGenericRepository<RefreshToken> RefreshTokens { get; }

    public IGenericRepository<Asset> Assets { get; }

    public IGenericRepository<Currency> Currencies { get; }

    public IGenericRepository<CurrencyPair> CurrencyPairs { get; }

    public IGenericRepository<Watchlist> Watchlists { get; }

    public IGenericRepository<WatchlistAsset> WatchlistAssets { get; }

    public IGenericRepository<WatchlistCurrencyPair> WatchlistCurrencyPairs { get; }

    public Task<int> SaveChanges()
    {
        return _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
