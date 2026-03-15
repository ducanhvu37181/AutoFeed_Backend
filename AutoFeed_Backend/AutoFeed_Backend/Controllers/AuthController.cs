using AutoFeed_Backend_Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string usernameOrEmail, string password)
    {
        var user = await _unitOfWork.Users.LoginAsync(usernameOrEmail, password);

        if (user == null)
        {
            return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
        }

        return Ok(new
        {
            message = "Đăng nhập thành công!",
            user = new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.FullName,
                user.RoleId
            }
        });
    }
}