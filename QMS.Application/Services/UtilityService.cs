using QMS.Application.Abstracts;
using QMS.Domain.Models;
using System.Text.RegularExpressions;

namespace QMS.Application.Services
{
    public class UtilityService : IUtilityService
    {
        public bool TryValidatePassword(string password, out string error)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                error = "Password must be at least 8 characters long.";
                return false;
            }
            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                error = "Password must contain at least one uppercase letter.";
                return false;
            }
            if (!Regex.IsMatch(password, "[a-z]"))
            {
                error = "Password must contain at least one lowercase letter.";
                return false;
            }
            if (!Regex.IsMatch(password, @"\d"))
            {
                error = "Password must contain at least one digit.";
                return false;
            }
            if (!Regex.IsMatch(password, @"[@$!%*?&_]"))
            {
                error = "Password must contain at least one special character (@ $ ! % * ? & _).";
                return false;
            }

            error = null!;
            return true;
        }

        // FIX: was using an out-param that was never reflected back to the caller because C# strings
        // are passed by value. The hashed string was silently discarded. Now returns the hash directly.
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty");

            // BCrypt silently truncates passwords longer than 72 bytes, so guard against it.
            if (System.Text.Encoding.UTF8.GetByteCount(password) > 72)
                throw new ArgumentException("Password is too long (max 72 bytes)");

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public void MappingFullName(User user, string fullName)
        {
            var parts = fullName.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = parts[0];
            user.SecondName = parts[1];
            user.ThirdName = parts.Length >= 3 ? parts[2] : null;
            user.FourthName = parts.Length >= 4 ? string.Join(" ", parts.Skip(3)) : null;
        }
    }
}