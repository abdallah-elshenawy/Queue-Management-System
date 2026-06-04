
namespace QMS.Application.DTOs.Ticket
{
    public class CreateTicket
    {
        public int ServiceId { get; set; } // FK
        public int BranchId { get; set; } // FK
    }
}
