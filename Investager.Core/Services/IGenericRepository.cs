using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAll();

        Task<TEntity> GetById(uint id);

        Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> filter, string includeProperties = "");

        Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, int take);

        void Insert(TEntity entity);

        void Update(TEntity entityToUpdate);

        void Delete(uint id);

        void Delete(TEntity entityToDelete);
    }
}
