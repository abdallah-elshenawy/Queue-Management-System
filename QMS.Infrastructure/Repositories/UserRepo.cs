using Microsoft.EntityFrameworkCore;
using QMS.Domain.Enums;
using QMS.Domain.IRepositories;
using QMS.Domain.Models;

namespace QMS.Infrastructure.Repositories
{
    public class UserRepo : BaseRepository<User>, IUserRepo
    {
        private readonly ApplicationDbContext context;

        public UserRepo(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            return await context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }
        public bool CheckPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        public async Task<bool> IsFoundAsync(string email)
        {
            return await context.Users.AnyAsync(u => u.Email == email);
        }
        //Task<List<T>> GetByRoleAsync(Role role, params Expression<Func<T, object>>[] includes);
        public async Task<List<User>> GetByRoleAsync(Role role, bool includeInfo = false)
        {
            var query = context.Users.AsNoTracking().Where(u => u.Role == role);
            if (includeInfo) query = query.Include(u => u.EmployeeInfo).Include(u => u.CustomerInfo);
            return await query.ToListAsync();
        }

        public async Task<bool> IsEmailExist(string email) => await context.Users.AnyAsync(u => u.Email == email);
       
        public async Task<bool> IsUsernameExist(string username) => await context.Users.AnyAsync(u => u.Username == username);

        public async Task<bool> IsNationalNumberExist(string nationalNumber) => await context.Users.AnyAsync(u => u.NationalNumber == nationalNumber);
    }
}
