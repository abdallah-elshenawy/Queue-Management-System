using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.Models
{
    public class EmployeeInfo
    {
        public int UserId { get; set; }
        public int? CounterNumber { get; set; }     // nullabe for door verifier
        public int? ServiceId { get; set; }          
        public int BranchId { get; set; }

        public User? User { get; set; }
        public Service? Service { get; set; }
        public ICollection<Ticket>? HandeledTickets { get; set; }
        public Branch? Branch { get; set; }
    }
}
