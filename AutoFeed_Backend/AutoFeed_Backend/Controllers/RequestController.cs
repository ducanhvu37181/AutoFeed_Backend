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
        Status = r.Status != null && (r.Status.ToLower() == "pending" || r.Status.ToLower() == "approved"),
        CreatedAt = r.CreatedAt
    };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllRequestsAsync();
        var dto = items.Select(ToDto).ToList();
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var items = await _service.GetActiveRequestsAsync();
        var dto = items.Select(ToDto).ToList();
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive()
    {
        var items = await _service.GetInactiveRequestsAsync();
        var dto = items.Select(ToDto).ToList();
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var items = await _service.SearchRequestsAsync(q);
        var dto = items.Select(ToDto).ToList();
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetRequestByIdAsync(id);
        if (item == null)
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Not Found"
            };
            return NotFound(error);
        }
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = ToDto(item),
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequestRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Type))
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            };
            return BadRequest(error);
        }

        var entity = new Request
        {
            UserId = model.UserId ?? 0,
            Type = model.Type,
            Description = model.Description
        };

        var id = await _service.CreateRequestAsync(entity);
        if (id <= 0)
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Create failed"
            };
            return StatusCode(500, error);
        }

        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 201,
            Data = ToDto(entity),
            Description = "Created"
        };
        return CreatedAtAction(nameof(Get), new { id = entity.RequestId }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRequestRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Type))
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            };
            return BadRequest(error);
        }

        var existing = await _service.GetRequestByIdAsync(id);
        if (existing == null)
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Not Found"
            };
            return NotFound(error);
        }

        existing.UserId = model.UserId ?? existing.UserId;
        existing.Type = model.Type;
        existing.Description = model.Description;
        if (!string.IsNullOrWhiteSpace(model.Status))
            existing.Status = model.Status;

        var ok = await _service.UpdateRequestAsync(existing);
        if (!ok)
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Update failed"
            };
            return StatusCode(500, error);
        }

        var successResponse = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = ToDto(existing),
            Description = "Update success"
        };
        return Ok(successResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteRequestAsync(id);
        if (!ok)
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Not Found or Delete failed"
            };
            return NotFound(error);
        }

        var success = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = null,
            Description = "Delete success"
        };
        return Ok(success);
    }
}