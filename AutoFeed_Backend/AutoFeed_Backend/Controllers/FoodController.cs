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
            ItemID = f.FoodId,
            Name = f.Name,
            Category = f.Type,
            Quantity = f.Quantity,
            MinStock = 200, // Gia su mac dinh theo Figma
            Status = f.Quantity <= 200 ? "Low Stock" : "In Stock",
            LastRestocked = DateTime.Now.ToString("yyyy-MM-dd"), // Lay tam ngay hien tai
            Supplier = "Nha cung cap mac dinh"
        });

        return Ok(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddItem(Food item)
    {
        if (item == null) return BadRequest();

        await _unitOfWork.Foods.CreateAsync(item);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();

        if (result > 0) return Ok(new { message = "Thêm vật tư thành công!" });
        return BadRequest(new { message = "Lỗi khi thêm vật tư" });
    }
}