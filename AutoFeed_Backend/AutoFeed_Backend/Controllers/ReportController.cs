using AutoFeed_Backend.Models.Requests.Report;
using AutoFeed_Backend.Models.Responses;
using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _service;

    public ReportController(IReportService service)
    {
        _service = service;
    }


    private static ReportResponse ToDto(Report r) => new()
    {
        ReportId = r.ReportId,
        UserId = r.UserId,
        UserName = r.User?.FullName,
        UserRole = r.User?.Role?.Description,
        Type = r.Type,
        Description = r.Description,
        Status = r.Status,
        CreateDate = r.CreateDate
    };

 
    [HttpGet("all")]
    public async Task<IActionResult> GetAllReports()
    {
        var items = await _service.GetAllReportsAsync();
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = items.Select(ToDto),
            Description = "Success"
        });
    }


    [HttpGet("farmer")]
    public async Task<IActionResult> GetFarmerReports()
    {
        var items = await _service.GetFarmerReportsAsync();
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = items.Select(ToDto),
            Description = "Success"
        });
    }


    [HttpGet("tech-farmer")]
    public async Task<IActionResult> GetTechFarmerReports()
    {
        var items = await _service.GetTechFarmerReportsAsync();
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = items.Select(ToDto),
            Description = "Success"
        });
    }


    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetReportsByStatus(string status)
    {
        var allowed = new[] { "pending", "approved", "rejected" };
        if (!allowed.Contains(status.ToLower()))
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid status. Allowed: pending, approved, rejected"
            });

        var items = await _service.GetReportsByStatusAsync(status);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = items.Select(ToDto),
            Description = "Success"
        });
    }


    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetReportsByUserId(int userId)
    {
        var items = await _service.GetReportsByUserIdAsync(userId);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = items.Select(ToDto),
            Description = "Success"
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReportById(int id)
    {
        var item = await _service.GetReportByIdAsync(id);
        if (item == null)
            return NotFound(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Report not found"
            });

        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = ToDto(item),
            Description = "Success"
        });
    }

    // Tìm kiếm report theo năm (bắt buộc), kết hợp tuỳ chọn: type, description, userId
    // Ví dụ: GET /api/report/search?year=2024&amp;type=health&amp;userId=3
    
    [HttpGet("search")]
    public async Task<IActionResult> SearchReportsByYear(
        [FromQuery] int year,
        [FromQuery] string? type,
        [FromQuery] string? description,
        [FromQuery] int? userId)
    {
        if (year < 2000 || year > DateTime.UtcNow.Year + 1)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid year"
            });

        var items = await _service.SearchReportsByYearAsync(year, type, description, userId);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = items.Select(ToDto),
            Description = "Success"
        });
    }

 
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Type))
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            });

        var entity = new Report
        {
            UserId = model.UserId,
            Type = model.Type,
            Description = model.Description
        };

        var id = await _service.CreateReportAsync(entity);
        if (id <= 0)
            return StatusCode(500, new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Create failed"
            });

        return CreatedAtAction(nameof(GetReportById), new { id = entity.ReportId },
            new ApiResponse<object>
            {
                Status = true,
                HttpCode = 201,
                Data = ToDto(entity),
                Description = "Created"
            });
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateReportRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Type))
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            });

        var existing = await _service.GetReportByIdAsync(id);
        if (existing == null)
            return NotFound(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Report not found"
            });

        existing.Type = model.Type;
        existing.Description = model.Description;
        if (!string.IsNullOrWhiteSpace(model.Status))
            existing.Status = model.Status;

        var ok = await _service.UpdateReportAsync(existing);
        if (!ok)
            return StatusCode(500, new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Update failed"
            });

        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = ToDto(existing),
            Description = "Update success"
        });
    }


    /// Cập nhật trạng thái report pending | approved | rejected

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] UpdateReportStatusRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Status))
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            });

        var ok = await _service.UpdateReportStatusAsync(id, model.Status);
        if (!ok)
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Report not found or invalid status. Allowed: pending, approved, rejected"
            });

        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = null,
            Description = "Status updated successfully"
        });
    }

  
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var ok = await _service.DeleteReportAsync(id);
        if (!ok)
            return NotFound(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Report not found or delete failed"
            });

        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = null,
            Description = "Delete success"
        });
    }
}




    