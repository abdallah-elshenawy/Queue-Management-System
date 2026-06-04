using QMS.Application.DTOs.Ticket;
using QMS.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.Branch
{
    public class GetBranch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public bool? IsActive { get; set; } = true;

        public ICollection<GetEmployee>? Employees { get; set; }
        public ICollection<GetTicket>? Tickets { get; set; }

    }
}
