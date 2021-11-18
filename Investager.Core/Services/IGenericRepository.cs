using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Investager.Core.Services;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAll();

    Task<IEnumerable<TEntity>> GetAll(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include);

    Task<TEntity> GetByIdWithTracking(int id);

    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> filter);

    Task<IEnumerable<TEntity>> Find(
        Expression<Func<TEntity, bool>> filter, Func<IQueryable<TEntity>,
        IIncludableQueryable<TEntity, object>> include);

    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> filter, int take);

    Task<IEnumerable<TEntity>> Find(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        int take);

    Task<IEnumerable<TEntity>> FindWithTracking(Expression<Func<TEntity, bool>> filter);

    void Add(TEntity entity);

    void Update(TEntity entityToUpdate);

    void Delete(int id);

    void Delete(Expression<Func<TEntity, bool>> filter);

    void Delete(TEntity entityToDelete);
}
