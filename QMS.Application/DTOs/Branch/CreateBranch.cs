using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.Branch
{
    public class CreateBranch
    {
        [Length(2, 50)]
        public string Name { get; set; }

        [Length(2, 50)]
        public string Address { get; set; }

        [Length(2, 20)]
        public string Contact { get; set; }
    }
}
