using QMS.Domain.IRepositories;
using QMS.Domain.Models;

namespace QMS.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        public IUserRepo UserRepo { get; private set; }

        public IRefreshTokenRepo RefreshTokenRepo { get; private set; }

        public IBaseRepository<CustomerInfo> CustomerRepo { get; private set; }

        public IBaseRepository<EmployeeInfo> EmployeeRepo { get; private set; }

        public IBaseRepository<Branch> BranchRepo { get; private set; }

        public ITicketRepo TicketRepo { get; private set; }

        public IBaseRepository<Service> ServiceRepo { get; private set; }

        public IDisplayScreenRepo DisplayScreen { get; private set; }

        public IBaseRepository<DisplayTicket> DisplayTicket { get; private set; }
        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            UserRepo = new UserRepo(context);
            RefreshTokenRepo = new RefreshTokenRepo(context);
            CustomerRepo = new BaseRepository<CustomerInfo>(context);
            EmployeeRepo = new BaseRepository<EmployeeInfo>(context);
            BranchRepo = new BaseRepository<Branch>(context);
            TicketRepo = new TicketRepo(context);
            ServiceRepo = new BaseRepository<Service>(context);
            DisplayScreen = new DisplayScreenRepo(context);
            DisplayTicket = new BaseRepository<DisplayTicket>(context);

        }
        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
        public void Dispose()
        {
            context.Dispose();
        }
    }
}
