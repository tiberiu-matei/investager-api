using Microsoft.EntityFrameworkCore;

namespace Investager.Infrastructure.Persistence
{
    public class InvestagerContext : DbContext
    {
        public InvestagerContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
