using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.Service
{
    public class CreateService
    {
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }
    }
}
