using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.User
{
    public class UpdateEmployee
    {
        public int CounterNumber { get; set; }
        public int ServiceId { get; set; }
        public int BranchId { get; set; }
    }
}
