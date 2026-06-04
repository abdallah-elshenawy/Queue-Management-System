using QMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.Models
{



    // admin
    public class User
    {
        public int Id { get; set; }

        [MaxLength(30)]
        public string Username { get; set; }

        [MaxLength(30)]
        public string FirstName { get; set; }   

        [MaxLength(30)]
        public string SecondName { get; set; }

        [MaxLength(30)]
        public string? ThirdName { get; set; }   

        [MaxLength(30)]

        public string? FourthName { get; set; }
        public string? ImageUrl { get; set; }

        [MaxLength(60)]
        public string Email { get; set; }
        public string NationalNumber { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public bool? IsActive { get; set; } = true;
        public Role Role { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;


        public EmployeeInfo? EmployeeInfo { get; set; }
        public CustomerInfo? CustomerInfo { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}


