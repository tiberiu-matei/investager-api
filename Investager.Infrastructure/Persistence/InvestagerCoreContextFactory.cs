using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Investager.Infrastructure.Persistence
{
    public class InvestagerCoreContextFactory : IDesignTimeDbContextFactory<InvestagerCoreContext>
    {
        public InvestagerCoreContext CreateDbContext(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<InvestagerCoreContext>();

            dbContextBuilder.UseNpgsql(args[0]);

            return new InvestagerCoreContext(dbContextBuilder.Options);
        }
    }
}
