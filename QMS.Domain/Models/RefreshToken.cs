namespace QMS.Domain.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(15);
        public int UserId { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FIX: was lowercase "user" — public properties should be PascalCase in C#
        public User? User { get; set; }
    }
}