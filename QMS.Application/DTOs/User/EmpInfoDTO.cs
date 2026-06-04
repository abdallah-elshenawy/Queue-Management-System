using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.User
{
    // used with GetUser DTO
    public class EmpInfoDTO
    {
        public int? CounterNumber { get; set; }     // nullabe for door verifier
        public int ServiceId { get; set; }          // i make it nullable in the database to allow door verifiers
        public int BranchId { get; set; }
    }
}
