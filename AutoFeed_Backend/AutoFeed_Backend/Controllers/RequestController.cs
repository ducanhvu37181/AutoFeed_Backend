using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Models.Requests.Request;
using AutoFeed_Backend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestController : ControllerBase
{
    private readonly IRequestService _service;

    public RequestController(IRequestService service)
    {
        _service = service;
    }

    private RequestResponse ToDto(Request r) => new RequestResponse
    {
        RequestId = r.RequestId,
        UserId = r.UserId,
        Type = r.Type,
        Description = r.Description,
        Status = r.Status,
        CreatedAt = r.CreatedAt
    };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllRequestsAsync();
        var dto = items.Select(ToDto).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var items = await _service.GetActiveRequestsAsync();
        var dto = items.Select(ToDto).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive()
    {
        var items = await _service.GetInactiveRequestsAsync();
        var dto = items.Select(ToDto).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var items = await _service.SearchRequestsAsync(q);
        var dto = items.Select(ToDto).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetRequestByIdAsync(id);
        if (item == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = ToDto(item), Description = "Success" });
    }

    // GET api/request/user/5  — lấy tất cả request của 1 user
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var items = await _service.GetByUserIdAsync(userId);
        var dto = items.Select(ToDto).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequestRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Type))
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var entity = new Request
        {
            UserId = model.UserId,
            Type = model.Type,
            Description = model.Description
        };

        var id = await _service.CreateRequestAsync(entity);
        if (id <= 0)
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Create failed" });

        return CreatedAtAction(nameof(Get), new { id = entity.RequestId },
            new ApiResponse<object> { Status = true, HttpCode = 201, Data = ToDto(entity), Description = "Created" });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRequestRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Type))
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var existing = await _service.GetRequestByIdAsync(id);
        if (existing == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });

        if (model.UserId.HasValue)
            existing.UserId = model.UserId.Value;
        existing.Type = model.Type;
        existing.Description = model.Description;
        if (!string.IsNullOrWhiteSpace(model.Status))
            existing.Status = model.Status;

        var ok = await _service.UpdateRequestAsync(existing);
        if (!ok)
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Update failed" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = ToDto(existing), Description = "Update success" });
    }

    // PATCH api/request/5/status  — chỉ cập nhật status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRequestStatusRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Status))
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var ok = await _service.UpdateStatusAsync(id, model.Status);
        if (!ok)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Not Found or invalid status. Allowed: pending, approved, rejected"
            });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "Status updated" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteRequestAsync(id);
        if (!ok)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found or Delete failed" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "Delete success" });
    }
}