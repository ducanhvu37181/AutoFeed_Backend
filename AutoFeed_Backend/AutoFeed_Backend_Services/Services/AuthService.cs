using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore; 
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace AutoFeed_Backend_Services.Services;

public class AuthService : IAuthService
{
    private readonly AutoFeedDBContext _context; 

    public AuthService(AutoFeedDBContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<User?> LoginAsync(string usernameOrEmail, string password)
    {
        // Kiểm tra cả Username và Email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && u.Status == true);

        if (user == null) return null;

        // Kiểm tra mật khẩu mã hóa BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

        return user;
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("Chuoi_Key_Bi_Mat_Cua_Kien_SE170416_FPT_University");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim("UserId", user.UserId.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}