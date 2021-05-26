using Investager.Core.Models;
using Investager.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Investager.Infrastructure.Persistence
{
    public class InvestagerTimeSeriesContext : DbContext
    {
        public DbSet<TimeSeriesPoint> TimeSeriesPoints { get; set; }

        public InvestagerTimeSeriesContext(DbContextOptions<InvestagerTimeSeriesContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TimeSeriesPointConfiguration());
        }
    }
}
