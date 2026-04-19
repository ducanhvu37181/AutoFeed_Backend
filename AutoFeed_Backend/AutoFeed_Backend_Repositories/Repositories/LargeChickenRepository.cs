using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class LargeChickenRepository : GenericRepository<LargeChicken>
{
    public LargeChickenRepository() : base() { }

    public LargeChickenRepository(AutoFeedDBContext context) : base(context) { }

    public new async Task<List<LargeChicken>> GetAllAsync()
    {
        return await _context.Set<LargeChicken>()
            .Include(lc => lc.ChickenBarn)
            .Include(lc => lc.Flock)
            .ToListAsync();
    }

    public new async Task<LargeChicken?> GetByIdAsync(int id)
    {
        return await _context.Set<LargeChicken>()
            .Include(lc => lc.ChickenBarn)
            .Include(lc => lc.Flock)
            .FirstOrDefaultAsync(x => x.ChickenLid == id);
    }

    public async Task<List<LargeChicken>> GetActiveAsync()
    {
        return await _context.Set<LargeChicken>()
            .Include(lc => lc.ChickenBarn)
            .Include(lc => lc.Flock)
            .Where(x => x.IsActive == true)
            .ToListAsync();
    }

    public async Task<List<LargeChicken>> GetInactiveAsync()
    {
        return await _context.Set<LargeChicken>()
            .Include(lc => lc.ChickenBarn)
            .Include(lc => lc.Flock)
            .Where(x => x.IsActive != true)
            .ToListAsync();
    }

    public async Task<List<LargeChicken>> SearchAsync(string? name, string? healthStatus, int? flockId, bool includeInactive)
    {
        var query = _context.Set<LargeChicken>()
            .Include(lc => lc.ChickenBarn)
            .Include(lc => lc.Flock)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(x => x.IsActive == true);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(x => x.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(healthStatus))
            query = query.Where(x => x.HealthStatus.Contains(healthStatus));

        if (flockId.HasValue)
            query = query.Where(x => x.FlockId == flockId.Value);

        return await query.ToListAsync();
    }
}