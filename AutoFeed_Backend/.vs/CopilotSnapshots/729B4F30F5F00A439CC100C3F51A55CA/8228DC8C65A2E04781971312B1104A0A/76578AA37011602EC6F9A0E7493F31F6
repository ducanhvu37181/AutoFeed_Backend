using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login(string usernameOrEmail, string password)
    {
        var user = await _authService.LoginAsync(usernameOrEmail, password);
        if (user == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });

        var token = _authService.GenerateJwtToken(user);
        return Ok(new
        {
            message = "Thành công!",
            token = token,
            user = new { user.UserId, user.Username, user.FullName, user.RoleId }
        });
    }
}