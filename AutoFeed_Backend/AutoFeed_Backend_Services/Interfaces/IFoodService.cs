using AutoFeed_Backend_DAO.Models;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IFoodService
{
    Task<IEnumerable<Food>> GetAllFoodsAsync();
    Task<Food?> GetFoodByIdAsync(int id);
    Task<bool> AddFoodItemAsync(Food item);
    Task<bool> UpdateFoodAsync(Food item);
    Task<bool> DeleteFoodAsync(int id); 
}