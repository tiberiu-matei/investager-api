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
            UserStarredAssets = new CoreGenericRepository<UserStarredAsset>(context);
        }

        public IGenericRepository<User> Users { get; }

        public IGenericRepository<RefreshToken> RefreshTokens { get; }

        public IGenericRepository<Asset> Assets { get; }

        public IGenericRepository<UserStarredAsset> UserStarredAssets { get; }

        public Task SaveChanges()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
