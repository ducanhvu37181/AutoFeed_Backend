using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IUserService
{
    // Read
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetActiveAsync();
    Task<List<User>> GetInactiveAsync();
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> SearchAsync(string? keyword, int? roleId, bool includeInactive);

    // Create
    Task<int> CreateAsync(User entity);

    // Update
    Task<bool> UpdateAsync(User entity);

    // Change password
    //Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    // Soft delete / Restore
    Task<bool> DeleteAsync(int id);   // status = false
    Task<bool> RestoreAsync(int id);  // status = true
}