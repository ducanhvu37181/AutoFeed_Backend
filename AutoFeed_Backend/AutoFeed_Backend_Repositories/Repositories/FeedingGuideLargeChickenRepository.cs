using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class FeedingGuideLargeChickenRepository : GenericRepository<FeedingGuideLargeChicken>
{
    public FeedingGuideLargeChickenRepository() : base() { }

    public FeedingGuideLargeChickenRepository(AutoFeedDBContext context) : base(context) { }

    public new async Task<List<FeedingGuideLargeChicken>> GetAllAsync()
    {
        return await _context.Set<FeedingGuideLargeChicken>()
            .ToListAsync();
    }

    public new async Task<FeedingGuideLargeChicken?> GetByIdAsync(int id)
    {
        return await _context.Set<FeedingGuideLargeChicken>()
            .FirstOrDefaultAsync(x => x.GuideLid == id);
    }

    public async Task<FeedingGuideLargeChicken?> GetByChickenTypeAndStatusAsync(string chickenType, string status)
    {
        return await _context.Set<FeedingGuideLargeChicken>()
            .FirstOrDefaultAsync(x => x.ChickenType == chickenType && x.Status == status);
    }

    public async Task<List<FeedingGuideLargeChicken>> SearchAsync(string? chickenType, string? status, decimal? weight)
    {
        var query = _context.Set<FeedingGuideLargeChicken>()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(chickenType))
            query = query.Where(x => x.ChickenType == chickenType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        if (weight.HasValue)
            query = query.Where(x => x.Weight == weight.Value);

        return await query.ToListAsync();
    }
}
