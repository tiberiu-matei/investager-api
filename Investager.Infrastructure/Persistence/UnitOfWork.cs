using Investager.Core.Models;
using Investager.Core.Services;
using System.Threading.Tasks;

namespace Investager.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InvestagerContext _context;

        public UnitOfWork(InvestagerContext context)
        {
            _context = context;
            Users = new GenericRepository<User>(context);
            RefreshTokens = new GenericRepository<RefreshToken>(context);
        }

        public IGenericRepository<User> Users { get; }

        public IGenericRepository<RefreshToken> RefreshTokens { get; }

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
