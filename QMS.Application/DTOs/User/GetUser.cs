using QMS.Domain.Enums;
using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.User
{
    public class GetUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string? ImageUrl { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string NationalNumber { get; set; }
        public string City { get; set; }

        public EmpInfoDTO? EmpInfo { get; set; }
    }
}
