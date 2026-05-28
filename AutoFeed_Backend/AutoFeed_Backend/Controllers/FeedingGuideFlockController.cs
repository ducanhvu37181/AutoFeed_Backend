using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend.Models.Responses;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedingGuideFlockController : ControllerBase
{
    private readonly IFeedingGuideFlockService _service;

    public FeedingGuideFlockController(IFeedingGuideFlockService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = result, Description = "Success" });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });
        
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = result, Description = "Success" });
    }

    [HttpGet("by-chicken-type/{chickenType}")]
    public async Task<IActionResult> GetByChickenType(string chickenType)
    {
        var result = await _service.GetByChickenTypeAsync(chickenType);
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = result, Description = "Success" });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FeedingGuideFlock entity)
    {
        if (entity == null)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        // Reset ID to let database auto-generate
        entity.GuideFid = 0;
        entity.CreatedAt = null;

        var result = await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = result.GuideFid },
            new ApiResponse<object> { Status = true, HttpCode = 201, Data = result, Description = "Created" });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FeedingGuideFlock entity)
    {
        if (entity == null)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });

        entity.GuideFid = id;
        var ok = await _service.UpdateAsync(entity);
        if (!ok)
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Update failed" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = entity, Description = "Update success" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "Deleted" });
    }
}
