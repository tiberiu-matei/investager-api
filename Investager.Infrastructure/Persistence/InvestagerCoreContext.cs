using Investager.Core.Models;
using Investager.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Investager.Infrastructure.Persistence;

public class InvestagerCoreContext : DbContext
{
    private readonly IConfiguration _configuration;

    public InvestagerCoreContext(IConfiguration configuration, DbContextOptions<InvestagerCoreContext> options) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_configuration[ConfigKeys.Environment] == Environments.Development)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AssetConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyPairConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WatchlistAssetConfiguration());
        modelBuilder.ApplyConfiguration(new WatchlistConfiguration());
        modelBuilder.ApplyConfiguration(new WatchlistCurrencyPairConfiguration());
    }
}
