using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class UserRepository : GenericRepository<User>
{
    public UserRepository() : base() { }
    public UserRepository(AutoFeedDBContext context) : base(context) { }

    public async Task<User?> LoginAsync(string identifier, string password)
    {
        // identifier có thể là Username HOẶC Email
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => (u.Username == identifier || u.Email == identifier)
                                 && u.Password == password
                                 && u.Status == true);
    }

    public async Task<List<User>> GetActiveAsync() => await _context.Set<User>().Where(u => u.Status == true).ToListAsync();
    public async Task<List<User>> GetInactiveAsync() => await _context.Set<User>().Where(u => u.Status != true).ToListAsync();
    public async Task<User?> GetByEmailAsync(string email) => await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
    public async Task<User?> GetByUsernameAsync(string username) => await _context.Set<User>().FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> IsEmailOrUsernameExistsAsync(string email, string username, int? excludeUserId = null)
    {
        var query = _context.Set<User>().AsQueryable();
        if (excludeUserId.HasValue) query = query.Where(u => u.UserId != excludeUserId.Value);
        return await query.AnyAsync(u => u.Email == email || u.Username == username);
    }

    public async Task<List<User>> SearchAsync(string? keyword, int? roleId, bool includeInactive)
    {
        var query = _context.Set<User>().AsQueryable();
        if (!includeInactive) query = query.Where(u => u.Status == true);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(u => (u.FullName != null && u.FullName.ToLower().Contains(kw)) ||
                                     (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                                     (u.Username != null && u.Username.ToLower().Contains(kw)));
        }
        if (roleId.HasValue) query = query.Where(u => u.RoleId == roleId.Value);
        return await query.ToListAsync();
    }

    /// <summary>Case-insensitive substring match on Role.Description (e.g. role=manager).</summary>
    public async Task<List<User>> GetAllByRoleDescriptionContainsAsync(string roleSubstring)
    {
        var term = roleSubstring.Trim().ToLowerInvariant();
        return await _context.Set<User>()
            .Include(u => u.Role)
            .Where(u => u.Role != null &&
                        u.Role.Description != null &&
                        u.Role.Description.ToLower().Contains(term))
            .ToListAsync();
    }
}