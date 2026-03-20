using AutoFeed_Backend_DAO.Models;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IAuthService
{
    Task<User?> LoginAsync(string usernameOrEmail, string password);
    string GenerateJwtToken(User user);
}