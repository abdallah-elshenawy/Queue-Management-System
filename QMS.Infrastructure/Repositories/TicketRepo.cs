using Microsoft.EntityFrameworkCore;
using QMS.Domain.Enums;
using QMS.Domain.IRepositories;
using QMS.Domain.Models;

namespace QMS.Infrastructure.Repositories
{
    public class TicketRepo : BaseRepository<Ticket>, ITicketRepo
    {
        private readonly ApplicationDbContext context;

        public TicketRepo(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Ticket> GetNextTicket(int serviceId, int branchId)
        {
            return await context.Tickets
                .Where(t => t.ServiceId == serviceId
                         && t.BranchId == branchId
                         && (t.Status == TicketStatus.Waiting || t.Status == TicketStatus.Requeued))
                .OrderBy(t => t.ReservedAt)
                .FirstOrDefaultAsync();
        }
    }
}