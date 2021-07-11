using Investager.Core.Models;
using Investager.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Investager.Infrastructure.Persistence
{
    public class InvestagerTimeSeriesContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<TimeSeriesPoint> TimeSeriesPoints { get; set; }

        public InvestagerTimeSeriesContext(IConfiguration configuration, DbContextOptions<InvestagerTimeSeriesContext> options) : base(options)
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
            modelBuilder.ApplyConfiguration(new TimeSeriesPointConfiguration());
        }
    }
}
