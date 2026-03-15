using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;

namespace AutoFeed_Backend_Repositories.Repositories;

public class FoodRepository : GenericRepository<Food>
{
    public FoodRepository() : base() { }
    public FoodRepository(AutoFeedDBContext context) : base(context) { }

    // API phục vụ giao diện Food & Inventory
    public async Task<List<Food>> GetFoodInventoryAsync(string? search, string? type)
    {
        var query = _context.Foods.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(f => f.Name.Contains(search));

        if (!string.IsNullOrEmpty(type) && type != "All Categories")
            query = query.Where(f => f.Type == type);

        return await query.ToListAsync();
    }
}