using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Investager.Infrastructure.Persistence
{
    public class InvestagerTimeSeriesContextFactory : IDesignTimeDbContextFactory<InvestagerTimeSeriesContext>
    {
        public InvestagerTimeSeriesContext CreateDbContext(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<InvestagerTimeSeriesContext>();

            dbContextBuilder.UseNpgsql(args[0]);

            return new InvestagerTimeSeriesContext(dbContextBuilder.Options);
        }
    }
}
