using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.Models
{
    public class DisplayTicket
    {
        public int Id { get; set; }
        public int DisplayId { get; set; } // FK to DisplayScreen
        public int TicketId { get; set; } // FK to Ticket
        public int? CounterNumber { get; set; }
        public DateTime CalledAt { get; set; }

        public DisplayScreen? Display { get; set; }
        public Ticket? Ticket { get; set; }
    }
}
