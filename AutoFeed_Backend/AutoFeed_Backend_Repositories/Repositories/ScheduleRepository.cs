using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class ScheduleRepository : GenericRepository<Schedule>
{
    public ScheduleRepository() : base() { }
    public ScheduleRepository(AutoFeedDBContext context) : base(context) { }

    public async Task<List<Schedule>> GetInProgressScheduleAsync()
    {
        // Status is stored as string: e.g. "pending", "inprogress", "completed".
        return await _context.Set<Schedule>()
            .Where(s => s.Status != null && s.Status.ToLower() == "inprogress")
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetCompletedScheduleAsync()
    {
        return await _context.Set<Schedule>()
            .Where(s => s.Status != null && s.Status.ToLower() == "completed")
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetPendingScheduleAsync()
    {
        return await _context.Set<Schedule>()
            .Where(s => s.Status != null && s.Status.ToLower() == "pending")
            .ToListAsync();
    }

    public async Task<List<Schedule>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await _context.Set<Schedule>().ToListAsync();

        query = query.Trim();
        if (int.TryParse(query, out var id))
        {
            return await _context.Set<Schedule>()
                .Where(s => s.SchedId == id || (s.Description != null && s.Description.Contains(query)))
                .ToListAsync();
        }

        return await _context.Set<Schedule>()
            .Where(s => s.Description != null && s.Description.Contains(query))
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetByUserIdAsync(int userId)
    {
        return await _context.Set<Schedule>()
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetByUserAndDateAsync(int userId, System.DateTime date)
    {
        var d = DateOnly.FromDateTime(date);
        return await _context.Set<Schedule>()
            .Where(s => s.UserId == userId
                && s.StartDate <= d
                && (s.EndDate == null || s.EndDate >= d))
            .ToListAsync();
    }
}
