using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.User
{
    public class UpdateUser
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
