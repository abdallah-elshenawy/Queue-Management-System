using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QMS.Application.Abstracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace QMS.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration config;

        public TokenService(IConfiguration config)
        {
            this.config = config;
        }
        public string GenerateAccessToken(int userId, string email, string role)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // this type of predefined claim to create a unique ID for the token so the generated token will be unique

            SecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));


            SigningCredentials credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            DateTime expiration = DateTime.UtcNow.AddMinutes(config.GetValue<int>("Jwt:TokenValidityInMinutes"));

            JwtSecurityToken token = new JwtSecurityToken
                (
                    claims: claims,
                    signingCredentials: credentials,
                    issuer: config["Jwt:IssuerUrl"],
                    audience: config["Jwt:AudienceUrl"],
                    expires: expiration
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}
