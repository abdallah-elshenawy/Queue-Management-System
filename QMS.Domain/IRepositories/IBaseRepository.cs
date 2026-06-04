using System.Linq.Expressions;

namespace QMS.Domain.IRepositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

        /// <summary>Read-only query by predicate. Uses AsNoTracking.</summary>
        Task<T> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Query by predicate with eager-loaded includes. Tracked — suitable before an update.
        /// Use this instead of GetByIdAsync for entities whose PK is not named "Id"
        /// (e.g. EmployeeInfo.UserId, CustomerInfo.UserId).
        /// </summary>
        Task<T> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> IsExist(int id);
    }
}