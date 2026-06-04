namespace QMS.Application.DTOs.Token
{
    // FIX: renamed from "getRT" — class names must be PascalCase and should be descriptive
    public class RefreshTokenRequest
    {
        public string Token { get; set; }
    }
}