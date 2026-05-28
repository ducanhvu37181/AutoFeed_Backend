using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class FeedingGuideFlockRepository : GenericRepository<FeedingGuideFlock>
{
    public FeedingGuideFlockRepository() : base() { }

    public FeedingGuideFlockRepository(AutoFeedDBContext context) : base(context) { }

    public new async Task<List<FeedingGuideFlock>> GetAllAsync()
    {
        return await _context.Set<FeedingGuideFlock>()
            .ToListAsync();
    }

    public new async Task<FeedingGuideFlock?> GetByIdAsync(int id)
    {
        return await _context.Set<FeedingGuideFlock>()
            .FirstOrDefaultAsync(x => x.GuideFid == id);
    }

    public async Task<FeedingGuideFlock?> GetByChickenTypeAndStatusAsync(string chickenType, string status)
    {
        return await _context.Set<FeedingGuideFlock>()
            .FirstOrDefaultAsync(x => x.ChickenType == chickenType && x.Status == status);
    }

    public async Task<List<FeedingGuideFlock>> SearchAsync(string? chickenType, string? status, int? age)
    {
        var query = _context.Set<FeedingGuideFlock>()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(chickenType))
            query = query.Where(x => x.ChickenType == chickenType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (age.HasValue)
            query = query.Where(x => x.Age == age.Value);

        return await query.ToListAsync();
    }
}
