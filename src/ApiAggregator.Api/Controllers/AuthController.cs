using ApiAggregator.Api.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ApiAggregator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private const string DemoUsername = "admin";
    private const string DemoPassword = "password";

    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions) {
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request) {
        if (request.Username != DemoUsername || request.Password != DemoPassword) {
            return Unauthorized();
        }

        return Ok(new LoginResponse {
            AccessToken = _jwtTokenService.GenerateAccessToken(request.Username),
            ExpiresInSeconds = _jwtOptions.ExpirationMinutes * 60
        });
    }
}
