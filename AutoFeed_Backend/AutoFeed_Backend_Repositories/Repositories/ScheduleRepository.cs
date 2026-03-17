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

    public async Task<List<Schedule>> GetInProgressTaskAsync()
    {
        // In-progress schedules are marked with Status == true and within the start/end window
        return await _context.Set<Schedule>()
            .Where(s => s.Status == true && (s.StartDate <= System.DateTime.Now && (s.EndDate == null || s.EndDate >= System.DateTime.Now)))
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetCompletedTaskAsync()
    {
        // Completed schedules should have Status == false and an EndDate in the past
        return await _context.Set<Schedule>()
            .Where(s => s.Status == false && s.EndDate != null && s.EndDate < System.DateTime.Now)
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
}
