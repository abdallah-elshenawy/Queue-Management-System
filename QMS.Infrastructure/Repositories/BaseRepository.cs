using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QMS.Domain.IRepositories;

namespace QMS.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext context;

        public BaseRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = context.Set<T>().AsNoTracking();
            foreach (var include in includes)
                query = query.Include(include);

            return await query.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            // NOTE: Only use this for entities whose primary key column is literally named "Id".
            // For EmployeeInfo / CustomerInfo (PK = UserId) use GetAsync(predicate, includes).
            var query = context.Set<T>().Where(row => EF.Property<int>(row, "Id") == id);
            foreach (var include in includes)
                query = query.Include(include);

            return await query.SingleOrDefaultAsync();
        }

        /// <summary>Read-only, no-tracking query by predicate.</summary>
        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await context.Set<T>().SingleOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Tracked query by predicate with optional eager-loaded includes.
        /// Use for entities that will be updated afterwards, or whose PK is not named "Id".
        /// </summary>
        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            var query = context.Set<T>().AsQueryable();
            foreach (var include in includes)
                query = query.Include(include);

            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task AddAsync(T entity)
        {
            await context.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            context.Remove(entity);
        }

        public void Update(T entity)
        {
            context.Update(entity);
        }

        public async Task<bool> IsExist(int id)
        {
            return await context.Set<T>()
                .Where(row => EF.Property<int>(row, "Id") == id)
                .AnyAsync();
        }
    }
}