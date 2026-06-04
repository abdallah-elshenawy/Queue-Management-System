using QMS.Domain.Enums;
using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.IRepositories
{
    public interface IUserRepo : IBaseRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        bool CheckPassword(string password, string passwordHash);
        Task<bool> IsFoundAsync(string email);
        Task<List<User>> GetByRoleAsync(Role role, bool includeInfo = false);
        Task<bool> IsEmailExist(string email);
        Task<bool> IsUsernameExist(string username);
        Task<bool> IsNationalNumberExist(string nationalNumber);

    }
}
