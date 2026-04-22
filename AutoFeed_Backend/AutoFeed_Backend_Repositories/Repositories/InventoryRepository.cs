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

    // DTO để bypass EF Core entity mapping
    public class InventoryDTO
    {
        public int InventId { get; set; }
        public string FoodName { get; set; }
        public int Quantity { get; set; }
        public decimal WeightPerBag { get; set; }
        public DateOnly ImportDate { get; set; }
        public DateOnly ExpiredDate { get; set; }
        public string Status { get; set; }
    }

    // Search inventory theo foodName
    public async Task<List<InventoryDTO>> SearchAsync(string? search)
    {
        string sql = "SELECT inventID, foodName, quantity, weightPerBag, importDate, expiredDate, status FROM Inventory";

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " WHERE foodName LIKE @search";
            var param = new Microsoft.Data.SqlClient.SqlParameter("@search", $"%{search}%");
            return await _context.Database.SqlQueryRaw<InventoryDTO>(sql, param).ToListAsync();
        }

        return await _context.Database.SqlQueryRaw<InventoryDTO>(sql).ToListAsync();
    }

    // Lấy inventory sắp hết hạn (trong vòng n ngày tới)
    public async Task<List<InventoryDTO>> GetExpiringSoonAsync(int days = 30)
    {
        var threshold = DateOnly.FromDateTime(DateTime.Today.AddDays(days));
        string sql = "SELECT inventID, foodName, quantity, weightPerBag, importDate, expiredDate, status FROM Inventory WHERE expiredDate <= @threshold";
        var param = new Microsoft.Data.SqlClient.SqlParameter("@threshold", threshold);
        return await _context.Database.SqlQueryRaw<InventoryDTO>(sql, param).ToListAsync();
    }

    // Lấy tất cả inventory chưa sử dụng (status = 'unused')
    public async Task<List<InventoryDTO>> GetUnusedInventoryAsync()
    {
        string sql = "SELECT inventID, foodName, quantity, weightPerBag, importDate, expiredDate, status FROM Inventory WHERE status = 'unused'";
        return await _context.Database.SqlQueryRaw<InventoryDTO>(sql).ToListAsync();
    }

    // Lấy tất cả inventory đã sử dụng (status = 'used')
    public async Task<List<InventoryDTO>> GetUsedInventoryAsync()
    {
        string sql = "SELECT inventID, foodName, quantity, weightPerBag, importDate, expiredDate, status FROM Inventory WHERE status = 'used'";
        return await _context.Database.SqlQueryRaw<InventoryDTO>(sql).ToListAsync();
    }
}