using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.Models
{
    public class Branch
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Contact { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public bool? IsActive { get; set; } = true;


        // Navigation Properties
        public ICollection<EmployeeInfo>? Employees { get; set; }
        public ICollection<Ticket>? Tickets { get; set; }   
        public DisplayScreen? Display { get; set; }
    }
}
