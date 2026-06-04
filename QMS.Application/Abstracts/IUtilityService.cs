using QMS.Domain.Models;

namespace QMS.Application.Abstracts
{
    public interface IUtilityService
    {
        bool TryValidatePassword(string password, out string error);
        // FIX: was bool HashPassword(string password, string hashPassword) — the out-param
        // pattern didn't work because strings are value types; callers got back nothing.
        string HashPassword(string password);
        void MappingFullName(User user, string fullName);
    }
}