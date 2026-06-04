using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepo UserRepo { get; }
        IRefreshTokenRepo RefreshTokenRepo { get; }
        IBaseRepository<CustomerInfo> CustomerRepo { get; }
        IBaseRepository<EmployeeInfo> EmployeeRepo { get; }
        IBaseRepository<Branch> BranchRepo { get; }
        ITicketRepo TicketRepo { get; }
        IBaseRepository<Service> ServiceRepo { get; }
        IDisplayScreenRepo DisplayScreen { get; }
        IBaseRepository<DisplayTicket> DisplayTicket { get; }
        Task<int> SaveChangesAsync();
    }
}
