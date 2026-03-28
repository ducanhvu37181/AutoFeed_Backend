using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class InventoryRepository : GenericRepository<Inventory>
{
    public InventoryRepository() : base() { }

    public InventoryRepository(AutoFeedDBContext context) : base(context) { }

    // Search inventory kèm thông tin Food
    public async Task<List<Inventory>> SearchAsync(string? search, string? type)
    {
        var query = _context.Inventories
            .Include(i => i.Food)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.Food.Name.Contains(search));

        if (!string.IsNullOrWhiteSpace(type) && type != "All Categories")
            query = query.Where(i => i.Food.Type == type);

        return await query.ToListAsync();
    }

    // Lấy inventory sắp hết hạn (trong vòng n ngày tới)
    public async Task<List<Inventory>> GetExpiringSoonAsync(int days = 30)
    {
        var threshold = DateOnly.FromDateTime(DateTime.Today.AddDays(days));
        return await _context.Inventories
            .Include(i => i.Food)
            .Where(i => i.ExpiredDate != null && i.ExpiredDate <= threshold)
            .ToListAsync();
    }
}