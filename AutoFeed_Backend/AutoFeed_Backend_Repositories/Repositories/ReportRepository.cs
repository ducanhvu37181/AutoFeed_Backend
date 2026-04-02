using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;

namespace AutoFeed_Backend_Repositories.Repositories;

public class ReportRepository : GenericRepository<Report>
{
    public ReportRepository() : base() { }
    public ReportRepository(AutoFeedDBContext context) : base(context) { }

    public async Task<List<Report>> GetAllWithUserAsync()
    {
        return await _context.Set<Report>()
            .Include(r => r.User)
            .OrderByDescending(r => r.CreateDate)
            .ToListAsync();
    }

    public async Task<Report?> GetByIdWithUserAsync(int id)
    {
        return await _context.Set<Report>()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReportId == id);
    }

    /// <summary>Lấy report theo Role của User (farmer / techfarmer)</summary>
    public async Task<List<Report>> GetByUserRoleAsync(string roleName)
    {
        return await _context.Set<Report>()
            .Include(r => r.User)
                .ThenInclude(u => u.Role)
            .Where(r => r.User != null
                     && r.User.Role != null
                     && r.User.Role.Description.ToLower().Contains(roleName.ToLower()))
            .OrderByDescending(r => r.CreateDate)
            .ToListAsync();
    }

    /// <summary>Lấy report theo Status: pending | approved | rejected</summary>
    public async Task<List<Report>> GetByStatusAsync(string status)
    {
        return await _context.Set<Report>()
            .Include(r => r.User)
            .Where(r => r.Status != null && r.Status.ToLower() == status.ToLower())
            .OrderByDescending(r => r.CreateDate)
            .ToListAsync();
    }

    /// <summary>Lấy report theo UserId</summary>
    public async Task<List<Report>> GetByUserIdAsync(int userId)
    {
        return await _context.Set<Report>()
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreateDate)
            .ToListAsync();
    }

    /// <summary>Tìm kiếm theo năm (và tuỳ chọn: type, description, userId)</summary>
    public async Task<List<Report>> SearchByYearAsync(int year, string? type, string? description, int? userId)
    {
        var query = _context.Set<Report>()
            .Include(r => r.User)
                .ThenInclude(u => u.Role)
            .Where(r => r.CreateDate.HasValue && r.CreateDate.Value.Year == year)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(r => r.Type != null && r.Type.Contains(type));

        if (!string.IsNullOrWhiteSpace(description))
            query = query.Where(r => r.Description != null && r.Description.Contains(description));

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        return await query.OrderByDescending(r => r.CreateDate).ToListAsync();
    }
}