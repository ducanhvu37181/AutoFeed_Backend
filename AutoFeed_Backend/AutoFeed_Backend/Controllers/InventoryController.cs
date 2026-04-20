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

    // GET api/Inventory?search=xxx&type=yyy
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

    // GET api/Inventory/expiring?days=30
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

    // GET api/Inventory/expired
    // Lấy danh sách thức ăn đã hết hạn
    [HttpGet("expired")]
    public async Task<IActionResult> GetExpired()
    {
        var data = await _service.GetExpiredInventoryAsync();
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = data,
            Description = "Success"
        });
    }

    // POST api/Inventory/add
    // Manager nhập kho: thêm một lô hàng mới vào inventory
    [HttpPost("add")]
    public async Task<IActionResult> AddInventory([FromBody] AddInventoryRequest model)
    {
        if (!DateOnly.TryParse(model.ExpiredDate, out var expiredDate))
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid date format. Use yyyy-MM-dd"
            });
        }

        var entity = new Inventory
        {
            FoodId = model.FoodId,
            Quantity = model.Quantity,
            WeightPerBag = model.WeightPerBag,
            ExpiredDate = expiredDate
        };

        try
        {
            var ok = await _service.AddInventoryAsync(entity);

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = new
                {
                    entity.FoodId,
                    entity.Quantity,
                    entity.ExpiredDate
                },
                Description = "Inventory added successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }


    // POST api/Inventory/request-item
    // Farmer gửi request lên manager để xin nhập thêm hàng
    //[HttpPost("request-item")]
    //public async Task<IActionResult> RequestNewItem([FromBody] RequestNewItemRequest model)
    //{
    //    if (model == null || model.UserId <= 0 || string.IsNullOrWhiteSpace(model.FoodName))
    //        return BadRequest(new ApiResponse<object>
    //        {
    //            Status = false,
    //            HttpCode = 400,
    //            Data = null,
    //            Description = "Invalid request"
    //        });

    //    var ok = await _service.RequestNewItemAsync(model.UserId, model.FoodName, model.Description);
    //    if (!ok)
    //        return StatusCode(500, new ApiResponse<object>
    //        {
    //            Status = false,
    //            HttpCode = 500,
    //            Data = null,
    //            Description = "Send request failed"
    //        });

    //    return Ok(new ApiResponse<object>
    //    {
    //        Status = true,
    //        HttpCode = 200,
    //        Data = null,
    //        Description = "Request sent successfully"
    //    });
    //}

    // POST api/Inventory/consume
    // Farmer xuất kho theo lịch cho gà ăn mà không cần gửi request đến manager
    //[HttpPost("consume")]
    //public async Task<IActionResult> Consume([FromBody] ConsumeInventoryRequest model)
    //{
    //    if (model.FoodId <= 0 || model.Quantity <= 0)
    //    {
    //        return BadRequest(new ApiResponse<object>
    //        {
    //            Status = false,
    //            HttpCode = 400,
    //            Description = "Invalid request"
    //        });
    //    }

    //    try
    //    {
    //        var ok = await _service.ConsumeInventoryAsync(model.FoodId, model.Quantity);

    //        return Ok(new ApiResponse<object>
    //        {
    //            Status = true,
    //            HttpCode = 200,
    //            Description = "Consume inventory successfully"
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new ApiResponse<object>
    //        {
    //            Status = false,
    //            HttpCode = 400,
    //            Description = ex.Message
    //        });
    //    }
    //}
    
    // GET api/Inventory/nearest-expired
    // Lấy patch gần hết hạn trước
    [HttpGet("nearest-expired")]
    public async Task<IActionResult> GetNearestExpired()
    {
        var data = await _service.GetNearestExpiredAsync();

        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = data,
            Description = "Success"
        });
    }

    // POST api/Inventory/start-feeding-session
    // Farmer xuất kho theo lịch cho gà ăn mà không cần gửi request đến manager, có kiểm tra lại xem dùng thực tế là bao nhiêu
    //[HttpPost("start-feeding-session")]
    //public async Task<IActionResult> StartFeeding([FromBody] StartFeedingSessionRequest model)
    //{
    //    var id = await _service.CreateFeedingSessionAsync(model.FoodId, model.Quantity);

    //    return Ok(new ApiResponse<object>
    //    {
    //        Status = true,
    //        HttpCode = 200,
    //        Data = new { SessionId = id },
    //        Description = "Session created"
    //    });
    //}

    // POST api/Inventory/complete-feeding-session
    // Farmer xuất kho theo lịch cho gà ăn mà không cần gửi request đến manager, có kiểm tra lại xem dùng thực tế là bao nhiêu
    //[HttpPost("complete-feeding-session")]
    //public async Task<IActionResult> CompleteFeeding([FromBody] CompleteFeedingSessionRequest model)
    //{
    //    await _service.CompleteFeedingSessionAsync(model.SessionId, model.ActualQuantity);

    //    return Ok(new ApiResponse<object>
    //    {
    //        Status = true,
    //        HttpCode = 200,
    //        Description = "Feeding completed"
    //    });
    //}

    // PUT api/Inventory/{id}
    // Cập nhật số lượng và ngày hết hạn của inventory
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateInventory(int id, [FromBody] UpdateInventoryRequest model)
    {
        if (model == null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            });
        }

        DateOnly? expiredDate = null;
        if (!string.IsNullOrWhiteSpace(model.ExpiredDate))
        {
            if (!DateOnly.TryParse(model.ExpiredDate, out var parsedDate))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "Invalid date format. Use yyyy-MM-dd"
                });
            }
            expiredDate = parsedDate;
        }

        try
        {
            var inventory = await _service.UpdateInventoryAsync(id, model.Quantity, expiredDate);

            if (inventory == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 404,
                    Data = null,
                    Description = "Inventory not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = new
                {
                    InventId = inventory.InventId,
                    FoodId = inventory.FoodId,
                    Quantity = inventory.Quantity,
                    WeightPerBag = inventory.WeightPerBag,
                    ExpiredDate = inventory.ExpiredDate.ToString("yyyy-MM-dd")
                },
                Description = "Inventory updated successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }
}