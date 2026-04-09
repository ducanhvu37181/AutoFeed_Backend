using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class ChickenBarnRepository : GenericRepository<ChickenBarn>
{
    public ChickenBarnRepository() : base() { }

    public ChickenBarnRepository(AutoFeedDBContext context) : base(context) { }

    public async Task<List<ChickenBarn>> GetActiveAsync()
    {
        return await _context.Set<ChickenBarn>()
            .Where(x => x.Status != null && x.Status.ToLower() == "active")
            .ToListAsync();
    }

    public async Task<List<ChickenBarn>> GetInactiveAsync()
    {
        return await _context.Set<ChickenBarn>()
            .Where(x => x.Status == null || x.Status.ToLower() != "active")
            .ToListAsync();
    }

    public async Task<List<ChickenBarn>> GetExportedAsync()
    {
        return await _context.Set<ChickenBarn>()
            .Where(x => x.ExportDate != null)
            .ToListAsync();
    }

    public async Task<List<ChickenBarn>> SearchAsync(int? barnId, int? flockId, int? chickenLid, bool includeInactive = false)
    {
        var query = _context.Set<ChickenBarn>().AsQueryable();

        if (!includeInactive)
            query = query.Where(x => x.Status != null && x.Status.ToLower() == "active");

        if (barnId.HasValue)
            query = query.Where(x => x.BarnId == barnId.Value);

        if (flockId.HasValue)
            query = query.Where(x => x.FlockId == flockId.Value);

        if (chickenLid.HasValue)
            query = query.Where(x => x.ChickenLid == chickenLid.Value);

        return await query.ToListAsync();
    }

    public async Task<List<ChickenBarn>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var list = ids?.ToList();
        if (list == null || list.Count == 0) return new List<ChickenBarn>();
        return await _context.Set<ChickenBarn>().Where(cb => list.Contains(cb.CbarnId)).ToListAsync();
    }
}
