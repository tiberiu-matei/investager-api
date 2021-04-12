using Investager.Core.Models;
using Investager.Core.Services;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Persistence
{
    public class CoreUnitOfWork : ICoreUnitOfWork
    {
        private readonly InvestagerCoreContext _context;

        public CoreUnitOfWork(InvestagerCoreContext context)
        {
            _context = context;
            Users = new CoreGenericRepository<User>(context);
            RefreshTokens = new CoreGenericRepository<RefreshToken>(context);
            Assets = new CoreGenericRepository<Asset>(context);
        }

        public IGenericRepository<User> Users { get; }

        public IGenericRepository<RefreshToken> RefreshTokens { get; }

        public IGenericRepository<Asset> Assets { get; }

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
