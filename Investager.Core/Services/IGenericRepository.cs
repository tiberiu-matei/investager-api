using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetById(uint id);

        Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> filter, string includeProperties = "");

        void Insert(TEntity entity);

        void Update(TEntity entityToUpdate);

        void Delete(uint id);

        void Delete(TEntity entityToDelete);
    }
}
