using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SmartTransportation.DAL.Models.Common;

namespace SmartTransportation.DAL.Repositories.Generic
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> GetQueryable();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);

        // ⭐ Save changes
        Task SaveAsync();

        // ⭐ Paged result
        Task<PagedResult<T>> GetPagedAsync(
          Expression<Func<T, bool>> filter,
          int pageNumber,
          int pageSize,
          Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
          params Expression<Func<T, object>>[] includeProperties
      );
    }
}
