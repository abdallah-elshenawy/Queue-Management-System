using QMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        public int CustomerId { get; set; } // FK
        public int ServiceId { get; set; } // FK
        public int BranchId { get; set; } // FK
        public int? EmployeeId { get; set; } // FK when employee calls ticket
        public string QRCodeData { get; set; } = Guid.NewGuid().ToString("N"); // This removes dashes and is shorter:
        public TicketStatus Status { get; set; } = TicketStatus.Waiting; // "Waiting", "Serving", "Completed", "Skipped", "Requeued"
        public DateTime? ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CalledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool? IsVerifiedAtDoor { get; set; } = false;
        public DateTime? VerificationTime { get; set; }



        // Navigation Properties
        public CustomerInfo? Customer { get; set; }
        public Service? Service { get; set; }
        public Branch? Branch { get; set; }
        public EmployeeInfo? Employee { get; set; }
        public ICollection<DisplayTicket>? DisplayTickets { get; set; }
    }
}
