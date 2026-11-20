using EsgTrainingManagement.Api.Security;
using EsgTrainingManagement.Api.DTOs;
using EsgTrainingManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsgTrainingManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(200)]
    [ProducesResponseType(401)]
    public ActionResult<TokenResponse> Login(LoginRequest request)
    {
        var token = service.Login(request);
        return token is null ? Unauthorized() : Ok(token);
    }
}
