using EsgTrainingManagement.Api.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgTrainingManagement.Api.Controllers;

public record LoginRequest(string Email, string Password);

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}

[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration configuration, JwtTokenService tokens) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(200)]
    [ProducesResponseType(401)]
    public ActionResult<TokenResponse> Login(LoginRequest request)
    {
        var user = configuration.GetSection("DemoUsers").Get<DemoUser[]>()?
            .SingleOrDefault(x => string.Equals(x.Email, request.Email, StringComparison.OrdinalIgnoreCase) && x.Password == request.Password);
        return user is null ? Unauthorized() : Ok(tokens.Create(user));
    }
}
