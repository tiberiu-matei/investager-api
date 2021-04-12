using Microsoft.EntityFrameworkCore;

namespace Investager.Infrastructure.Persistence
{
    public class InvestagerCoreContext : DbContext
    {
        public InvestagerCoreContext(DbContextOptions<InvestagerCoreContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AssetConfiguration());
        }
    }
}
