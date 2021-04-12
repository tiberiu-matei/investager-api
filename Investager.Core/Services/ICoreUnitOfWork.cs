using Investager.Core.Models;
using System;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface ICoreUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }

        IGenericRepository<RefreshToken> RefreshTokens { get; }

        IGenericRepository<Asset> Assets { get; }

        Task SaveChangesAsync();
    }
}
