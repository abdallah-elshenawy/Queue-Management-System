using QMS.Application.CustomValidators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.DTOs.User
{
    public class CreateDoorVerifier
    {
        [FullName]
        public string FullName { get; set; }


        [Length(minimumLength: 8, maximumLength: 30)]
        public string Username { get; set; }


        [EmailAddress]
        [Length(10, 50)]
        public string Email { get; set; }

        [RegularExpression(@"^(010|011|012|015)\d{8}$", ErrorMessage = "Invalid Egyptian phone number")]
        public string PhoneNumber { get; set; }

        [Length(minimumLength: 14, maximumLength: 14, ErrorMessage = "The national number should be 14")]
        public string NationalNumber { get; set; }
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Length(minimumLength: 2, 30)]
        public string City { get; set; }

        public int BranchId { get; set; }
    }
}
