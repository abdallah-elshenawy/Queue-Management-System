using QMS.Domain.Models;

namespace QMS.Domain.IRepositories
{
    public interface ITicketRepo : IBaseRepository<Ticket>
    {
        Task<Ticket> GetNextTicket(int serviceId, int branchId);
    }
}
