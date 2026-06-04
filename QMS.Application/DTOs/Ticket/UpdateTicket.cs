using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.Ticket
{
    public class UpdateTicket
    {
        public int ServiceId { get; set; } // FK
        public int BranchId { get; set; } // FK
    }
}
