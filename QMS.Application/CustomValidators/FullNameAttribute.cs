using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.CustomValidators
{
    // this attribute ensures that the full name contains at least 3 parts (e.g., first name, middle name, last name, etc.)
    public class FullNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) 
                return ValidationResult.Success;

            validationContext.DisplayName = "Full name";

            var parts = value.ToString().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 3 ? ValidationResult.Success
                                     : new ValidationResult($"{validationContext.DisplayName} must contain at least 3 names.");
        }
    }
}
