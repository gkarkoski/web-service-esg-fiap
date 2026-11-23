using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EsgTrainingManagement.Api.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "EsgTrainingManagement";
    public string Audience { get; set; } = "EsgTrainingManagement";
    public string SigningKey { get; set; } = "";
    public int ExpirationMinutes { get; set; } = 60;
}

public class DemoUser
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
}

public record TokenResponse(string Token, DateTime ExpiresAt);

public class JwtTokenService(JwtSettings settings, TimeProvider clock)
{
    public TokenResponse Create(DemoUser user)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(settings.ExpirationMinutes);
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Email), new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            now, expires,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
                SecurityAlgorithms.HmacSha256));
        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}