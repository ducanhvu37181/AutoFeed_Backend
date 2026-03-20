using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;

namespace AutoFeed_Backend_Services.Services;

public class FoodService : IFoodService
{
    private readonly IUnitOfWork _unitOfWork;

    public FoodService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<IEnumerable<object>> GetInventoryAsync(string? search, string? type)
    {
        var foods = await _unitOfWork.Foods.GetFoodInventoryAsync(search, type);
        return foods.Select(f => new {
            ItemID = "FD" + f.FoodId.ToString("D3"),
            Name = f.Name,
            Category = f.Type,
            Quantity = f.Quantity,
            MinStock = 50,
            Status = f.Quantity <= 50 ? "Low Stock" : "In Stock",
            LastRestocked = DateTime.Now.ToString("yyyy-MM-dd"),
            Supplier = f.Note ?? "Chưa có nhà cung cấp"
        });
    }

    public async System.Threading.Tasks.Task<bool> AddFoodItemAsync(Food item)
    {
        await _unitOfWork.Foods.CreateAsync(item);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }
}