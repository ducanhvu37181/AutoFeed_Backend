using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoodController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public FoodController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory(string? search, string? type)
    {
        var foods = await _unitOfWork.Foods.GetFoodInventoryAsync(search, type);

        var result = foods.Select(f => new
        {
            ItemID = "FD" + f.FoodId.ToString("D3"),
            Name = f.Name,
            Category = f.Type,
            Quantity = f.Quantity,
            MinStock = 50,
            Status = f.Quantity <= 50 ? "Low Stock" : "In Stock",
            LastRestocked = DateTime.Now.ToString("yyyy-MM-dd"),
            Supplier = f.Note ?? "Chưa có nhà cung cấp"
        });

        return Ok(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddItem(Food item)
    {
        if (item == null) return BadRequest();

        // Map các trường từ giao diện Add Inventory Item vào DB
        // item.Name ứng với Item Name
        // item.Type ứng với Category
        // item.Quantity ứng với Quantity
        // item.Note có thể dùng tạm để lưu Supplier 

        await _unitOfWork.Foods.CreateAsync(item);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();

        if (result > 0) return Ok(new { message = "Thêm vật tư thành công!" });
        return BadRequest(new { message = "Lỗi khi thêm vật tư" });
    }
}