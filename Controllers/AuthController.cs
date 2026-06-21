using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;
using WheresWaldoApi.Helpers;


namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost("register")]
    [EnableRateLimiting("Registration")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserDto dto)
    {
        var user =
            await _userService.RegisterAsync(dto);

        return Created(string.Empty, user);
    }

    [HttpPost("login")]
    [EnableRateLimiting("Login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserDto dto)
    {
        var result = await _userService.LoginAsync(dto);

        Response.Cookies.Append(
            "access_token",
            result.Token,
            CookieHelper.AuthCookie(
                HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
            )
        );

        return Ok(result.User);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
            "access_token"
        );

        return NoContent();
    }
}