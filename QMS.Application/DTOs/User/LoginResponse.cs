namespace QMS.Application.DTOs.User
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        // FIX: typo "Secnonds" → "Seconds"
        public int AccessTokenExpirationInSeconds { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}