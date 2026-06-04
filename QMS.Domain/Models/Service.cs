using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.Models
{
    public class Service
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }
        public bool? IsActive { get; set; } = true;


        // Navigation Properties
        public EmployeeInfo? Employee { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }
}
