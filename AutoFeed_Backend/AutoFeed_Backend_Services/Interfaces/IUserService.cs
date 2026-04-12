using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;

using Task = System.Threading.Tasks.Task;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IUserService
{
    /// <param name="roleDescriptionContains">If set, only users whose Role.Description contains this text (case-insensitive), e.g. "manager".</param>
    Task<List<User>> GetAllAsync(string? roleDescriptionContains = null);
    Task<List<User>> GetActiveAsync();
    Task<List<User>> GetInactiveAsync();
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> SearchAsync(string? keyword, int? roleId, bool includeInactive);

    Task<Dictionary<int, string>> GetUserNameMapAsync(IEnumerable<int> userIds);

    Task<int> CreateAsync(User entity);

    Task<bool> UpdateAsync(User entity);

    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string email);

    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);

    Task MigratePasswordsAsync();
}