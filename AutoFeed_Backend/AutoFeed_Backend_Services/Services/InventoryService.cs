using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Search food trong inventory — trả về object gọn cho frontend
    public async Task<IEnumerable<object>> SearchInventoryAsync(string? search, string? type)
    {
        var items = await _unitOfWork.Inventories.SearchAsync(search, type);

        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodId = i.FoodId,
            FoodName = i.Food?.Name,
            FoodType = i.Food?.Type,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            TotalWeight = i.Quantity * i.WeightPerBag,
            ExpiredDate = i.ExpiredDate?.ToString("yyyy-MM-dd") ?? "N/A",
            Status = i.ExpiredDate == null
    ? "Unknown"
    : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today)
        ? "Expired"
        : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            ? "Expiring Soon"
            : "Good",
        });
    }

    // Lấy danh sách sắp hết hạn
    public async Task<IEnumerable<object>> GetExpiringSoonAsync(int days = 30)
    {
        var items = await _unitOfWork.Inventories.GetExpiringSoonAsync(days);

        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodId = i.FoodId,
            FoodName = i.Food?.Name,
            FoodType = i.Food?.Type,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            ExpiredDate = i.ExpiredDate?.ToString("yyyy-MM-dd") ?? "N/A",
            DaysLeft = i.ExpiredDate?.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber
        });
    }

    // Nhập kho: thêm inventory mới
    public async Task<bool> AddInventoryAsync(Inventory item)
    {
        _unitOfWork.Inventories.PrepareCreate(item);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    // Farmer gửi request xin nhập thêm hàng — tạo Request với type = "Inventory"
    public async Task<bool> RequestNewItemAsync(int userId, string foodName, string description)
    {
        var request = new Request
        {
            UserId = userId,
            Type = "Inventory",
            Description = $"Yêu cầu nhập thêm: {foodName}. {description}".Trim(),
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Requests.PrepareCreate(request);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }
}