using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Models.Requests.Inventory;
using AutoFeed_Backend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    // GET api/inventory?search=xxx&type=yyy
    // Tìm kiếm food trong kho inventory, lọc theo tên và loại
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] string? type)
    {
        var data = await _service.SearchInventoryAsync(search, type);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = data,
            Description = "Success"
        });
    }

    // GET api/inventory/expiring?days=30
    // Lấy danh sách hàng sắp hết hạn trong n ngày tới
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringSoon([FromQuery] int days = 30)
    {
        var data = await _service.GetExpiringSoonAsync(days);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = data,
            Description = "Success"
        });
    }

    // POST api/inventory/add
    // Manager nhập kho: thêm một lô hàng mới vào inventory
    [HttpPost("add")]
    public async Task<IActionResult> AddInventory([FromBody] AddInventoryRequest model)
    {
        if (model == null || model.FoodId <= 0 || model.Quantity <= 0)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            });

        if (!DateOnly.TryParse(model.ExpiredDate, out var expiredDate))
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid date format. Use yyyy-MM-dd"
            });

        var entity = new Inventory
        {
            FoodId = model.FoodId,
            Quantity = model.Quantity,
            WeightPerBag = model.WeightPerBag,
            ExpiredDate = expiredDate
        };

        var ok = await _service.AddInventoryAsync(entity);
        if (!ok)
            return StatusCode(500, new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Add inventory failed"
            });

        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = null,
            Description = "Inventory added successfully"
        });
    }

    // POST api/inventory/request-item
    // Farmer gửi request lên manager để xin nhập thêm hàng
    [HttpPost("request-item")]
    public async Task<IActionResult> RequestNewItem([FromBody] RequestNewItemRequest model)
    {
        if (model == null || model.UserId <= 0 || string.IsNullOrWhiteSpace(model.FoodName))
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            });

        var ok = await _service.RequestNewItemAsync(model.UserId, model.FoodName, model.Description);
        if (!ok)
            return StatusCode(500, new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Send request failed"
            });

        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = null,
            Description = "Request sent successfully"
        });
    }
}