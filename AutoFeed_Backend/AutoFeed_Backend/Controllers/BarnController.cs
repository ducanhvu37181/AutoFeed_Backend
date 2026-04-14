using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BarnController : ControllerBase
{
    private readonly IBarnService _barnService;

    public BarnController(IBarnService barnService)
    {
        _barnService = barnService;
    }

    // Class to define exactly what fields show up in Swagger POST/PUT
    public class BarnRequest
    {
        // When creating a barn only Type and Area are required.
        public string Type { get; set; }
        public decimal Area { get; set; }
        public System.DateTime? CreateDate { get; set; }
    }

    // Request used for updating only Type and Area of a barn
    public class UpdateBarnInfoRequest
    {
        public int BarnId { get; set; }
        public string Type { get; set; }
        public decimal Area { get; set; }
    }

    public class UpdateBarnMetricsRequest
    {
        public int BarnId { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBarns()
    {
        var barns = await _barnService.GetAllAsync();
        var result = barns.Select(b => new {
            b.BarnId,
            b.Temperature,
            b.Humidity,
            b.FoodAmount,
            b.WaterAmount,
            b.Type,
            b.Area,
            b.CreateDate
        });
        return Ok(result);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableBarns()
    {
        var barns = await _barnService.GetAvailableAsync();
        var result = barns.Select(b => new {
            b.BarnId,
            b.Temperature,
            b.Humidity,
            b.FoodAmount,
            b.WaterAmount,
            b.Type,
            b.Area,
            b.CreateDate
        });
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = result, Description = "Success" });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBarnById(int id)
    {
        var b = await _barnService.GetByIdAsync(id);
        if (b == null) return NotFound("Barn not found.");

        return Ok(new
        {
            b.BarnId,
            b.Temperature,
            b.Humidity,
            b.FoodAmount,
            b.WaterAmount,
            b.Type,
            b.Area,
            b.CreateDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateBarn([FromBody] BarnRequest request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var barn = new Barn
        {
            Temperature = 0,
            Humidity = 0,
            FoodAmount = 0,
            WaterAmount = 0,
            Type = request.Type,
            Area = request.Area,
            CreateDate = DateTime.Now
        };

        try
        {
            var result = await _barnService.CreateBarnAsync(barn);
            if (!result) return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Create failed" });

            var dto = new { BarnId = barn.BarnId, Type = barn.Type, Area = barn.Area, Temperature = barn.Temperature, Humidity = barn.Humidity, FoodAmount = barn.FoodAmount, WaterAmount = barn.WaterAmount};
            var response = new ApiResponse<object> { Status = true, HttpCode = 201, Data = dto, Description = "Created" };
            return CreatedAtAction(nameof(GetBarnById), new { id = barn.BarnId }, response);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            // Provide inner exception message for easier debugging of constraint/SQL issues
            var message = dbEx.InnerException?.Message ?? dbEx.Message;
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = message });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateBarn([FromBody] UpdateBarnInfoRequest request)
    {
        if (request == null)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var existing = await _barnService.GetByIdAsync(request.BarnId);
        if (existing == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Barn not found" });

        // Only update Type and Area
        existing.Type = request.Type;
        existing.Area = request.Area;

        var ok = await _barnService.UpdateBarnAsync(existing);
        if (!ok) return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Update failed" });

        var dto = new { BarnId = existing.BarnId, Type = existing.Type, Area = existing.Area };
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Update success" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBarn(int id)
    {
        var result = await _barnService.DeleteBarnAsync(id);
        if (!result) return BadRequest("Failed to delete barn.");

        return Ok(result);
    }

    [HttpPatch("metrics")]
    public async Task<IActionResult> UpdateBarnMetrics([FromBody] UpdateBarnMetricsRequest req)
    {
        if (req == null) return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var ok = await _barnService.UpdateBarnMetricsAsync(req.BarnId, req.Temperature, req.Humidity);
        if (!ok) return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Barn not found or update failed" });

        // return the updated values
        var updated = await _barnService.GetByIdAsync(req.BarnId);
        var dto = new { BarnId = updated?.BarnId, Temperature = updated?.Temperature, Humidity = updated?.Humidity };
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Metrics updated" });
    }
}