using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.DisplayScreen
{
    public class GetDisplayTicket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; }
        public int CounterNumber { get; set; }
        public DateTime CalledAt { get; set; }

    }
}
