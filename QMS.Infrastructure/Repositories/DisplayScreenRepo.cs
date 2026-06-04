using Microsoft.EntityFrameworkCore;
using QMS.Domain.IRepositories;
using QMS.Domain.Models;

    namespace QMS.Infrastructure.Repositories
    {
        public class DisplayScreenRepo : BaseRepository<DisplayScreen>, IDisplayScreenRepo
        {
            private readonly ApplicationDbContext context;

            public DisplayScreenRepo(ApplicationDbContext context) : base(context)
            {
                this.context = context;
            }
            public async Task<List<DisplayTicket>> GetDisplayData(int branchId)
            {
                return await context.DisplayTickets.Include(dt => dt.Ticket).Include(dt => dt.Display).AsNoTracking().Where(dt => dt.Display.BranchId == branchId)
                    .OrderByDescending(dt => dt.CalledAt).Take(8).ToListAsync();
            }
        }
    }
