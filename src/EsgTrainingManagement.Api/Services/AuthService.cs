using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Security;

namespace EsgTrainingManagement.Api.Services;

public class AuthService(IConfiguration configuration, JwtTokenService tokens)
{
    public TokenResponse? Login(LoginRequest request)
    {
        var user = configuration.GetSection("DemoUsers").Get<DemoUser[]>()?
            .SingleOrDefault(x =>
                string.Equals(x.Email, request.Email, StringComparison.OrdinalIgnoreCase) &&
                x.Password == request.Password);
        return user is null ? null : tokens.Create(user);
    }
}