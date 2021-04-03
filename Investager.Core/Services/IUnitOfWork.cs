using Investager.Core.Models;
using System;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }

        IGenericRepository<RefreshToken> RefreshTokens { get; }

        Task SaveChanges();
    }
}
