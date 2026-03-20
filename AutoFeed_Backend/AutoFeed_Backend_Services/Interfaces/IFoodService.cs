using AutoFeed_Backend_DAO.Models;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IFoodService
{
    Task<IEnumerable<object>> GetInventoryAsync(string? search, string? type);
    Task<bool> AddFoodItemAsync(Food item);
}