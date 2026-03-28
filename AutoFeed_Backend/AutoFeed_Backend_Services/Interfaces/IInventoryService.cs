using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IInventoryService
{
    // Search food trong inventory 
    Task<IEnumerable<object>> SearchInventoryAsync(string? search, string? type);

    // Lấy inventory sắp hết hạn
    Task<IEnumerable<object>> GetExpiringSoonAsync(int days = 30);

    // Thêm inventory mới (nhập kho)
    Task<bool> AddInventoryAsync(Inventory item);

    // Farmer gửi request xin nhập thêm hàng cho manager
    Task<bool> RequestNewItemAsync(int userId, string foodName, string description);
}