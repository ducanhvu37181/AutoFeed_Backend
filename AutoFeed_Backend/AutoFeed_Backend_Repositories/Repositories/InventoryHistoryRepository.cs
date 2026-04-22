using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class InventoryHistoryRepository : GenericRepository<InventoryHistory>
{
    public InventoryHistoryRepository() : base() { }

    public InventoryHistoryRepository(AutoFeedDBContext context) : base(context) { }

    // DTO để bypass EF Core entity mapping
    public class InventoryHistoryDTO
    {
        public int HistoryId { get; set; }
        public int InventId { get; set; }
        public string FoodName { get; set; }
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
        public int QuantityChange { get; set; }
        public decimal WeightPerBag { get; set; }
        public DateOnly ImportDate { get; set; }
        public DateOnly ExpiredDate { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? ChangedBy { get; set; }
        public string ActionType { get; set; }
    }

    // Lấy lịch sử theo inventory ID
    public async Task<List<InventoryHistoryDTO>> GetHistoryByInventoryIdAsync(int inventId)
    {
        string sql = @"SELECT historyID, inventID, foodName, oldQuantity, newQuantity, quantityChange, 
                      weightPerBag, importDate, expiredDate, changedAt, changedBy, actionType 
                      FROM InventoryHistory 
                      WHERE inventID = @inventId 
                      ORDER BY changedAt DESC";
        var param = new Microsoft.Data.SqlClient.SqlParameter("@inventId", inventId);
        return await _context.Database.SqlQueryRaw<InventoryHistoryDTO>(sql, param).ToListAsync();
    }

    // Lấy tất cả lịch sử
    public async Task<List<InventoryHistoryDTO>> GetAllHistoryAsync()
    {
        string sql = @"SELECT historyID, inventID, foodName, oldQuantity, newQuantity, quantityChange, 
                      weightPerBag, importDate, expiredDate, changedAt, changedBy, actionType 
                      FROM InventoryHistory 
                      ORDER BY changedAt DESC";
        return await _context.Database.SqlQueryRaw<InventoryHistoryDTO>(sql).ToListAsync();
    }
}
