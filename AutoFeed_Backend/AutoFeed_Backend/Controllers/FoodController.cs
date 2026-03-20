using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoodController : ControllerBase
{
    private readonly IFoodService _foodService;
    public FoodController(IFoodService foodService) => _foodService = foodService;

    [HttpGet]
    public async Task<IActionResult> GetInventory(string? search, string? type)
        => Ok(await _foodService.GetInventoryAsync(search, type));

    [HttpPost("add")]
    public async Task<IActionResult> AddItem(Food item)
    {
        var result = await _foodService.AddFoodItemAsync(item);
        if (result) return Ok(new { message = "Thành công!" });
        return BadRequest(new { message = "Lỗi!" });
    }
}