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

    public async Task<List<User>> GetActiveAsync()
    {
        return await _context.Set<User>()
            .Where(u => u.Status == true)
            .ToListAsync();
    }

    public async Task<List<User>> GetInactiveAsync()
    {
        return await _context.Set<User>()
            .Where(u => u.Status != true)
            .ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> IsEmailOrUsernameExistsAsync(string email, string username, int? excludeUserId = null)
    {
        var query = _context.Set<User>().AsQueryable();

        if (excludeUserId.HasValue)
            query = query.Where(u => u.UserId != excludeUserId.Value);

        return await query.AnyAsync(u => u.Email == email || u.Username == username);
    }

    public async Task<List<User>> SearchAsync(string? keyword, int? roleId, bool includeInactive)
    {
        var query = _context.Set<User>().AsQueryable();

        if (!includeInactive)
            query = query.Where(u => u.Status == true);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(kw)) ||
                (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                (u.Username != null && u.Username.ToLower().Contains(kw)));
        }

        if (roleId.HasValue)
            query = query.Where(u => u.RoleId == roleId.Value);

        return await query.ToListAsync();
    }
}