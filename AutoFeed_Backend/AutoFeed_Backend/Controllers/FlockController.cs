using AutoFeed_Backend.Models.Requests.Flock;
using AutoFeed_Backend.Models.Responses;
using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlockController : ControllerBase
{
    private readonly IFlockService _flockService;
    private readonly ILargeChickenService _largeChickenService;
    private readonly IChickenBarnService _chickenBarnService;

    public FlockController(IFlockService flockService, ILargeChickenService largeChickenService, IChickenBarnService chickenBarnService)
    {
        _flockService = flockService;
        _largeChickenService = largeChickenService;
        _chickenBarnService = chickenBarnService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] string? searchTerm, [FromQuery] int? barnId, [FromQuery] string? status)
    {
        // Fallback to returning all flocks as dashboard data (service doesn't provide filtered dashboard)
        var data = await _flockService.GetAllFlocksAsync();
        return Ok(new { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var data = await _flockService.GetFlockByIdAsync(id);
        return data != null ? Ok(new { Success = true, Data = data }) : NotFound();
    }

    //[HttpPost]
    //public async Task<IActionResult> Create([FromBody] FlockChicken flock, [FromQuery] int barnId)
    //{
    //    var res = await _flockService.CreateFlockAsync(flock, barnId);
    //    return res ? Ok(new { Message = "Created" }) : BadRequest();
    //}

    //[HttpPut]
    //public async Task<IActionResult> Update([FromBody] FlockChicken flock)
    //{
    //    var res = await _flockService.UpdateFlockAsync(flock);
    //    return res ? Ok(new { Message = "Updated" }) : BadRequest();
    //}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _flockService.DeleteFlockAsync(id);
        return res ? Ok(new { Message = "Deleted" }) : NotFound();
    }

    [HttpPost("assign-flock-to-largeBarn")]
    public async Task<IActionResult> AssignIds([FromBody] AsssignToLargeChickRequest model)
    {
        //var res = await _flockService.AssignIdsToLargeChickensAsync(id);
        if (model == null)
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

        var largeChicken = new LargeChicken
        {
            FlockId = model.FlockId,
            Name = "LChikenFromFlock" + model.FlockId.ToString(),
            Weight = 0,
            HealthStatus = "Healthy",
            Note = model.Note,
            IsActive = true
        };

        var chickenLid = await _largeChickenService.CreateAsync(largeChicken);
        if (chickenLid <= 0)
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Failed to assign - could not create LargeChicken"
            };
            return StatusCode(500, error);
        }

        var chickenBarn = new ChickenBarn
        {
            BarnId = model.BarnId,
            ChickenLid = chickenLid,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            Note = model.Note,
            Status = "active"
        };

        var result = await _chickenBarnService.CreateAsync(chickenBarn);
        if (result == 0)
        {
            var error = new ApiResponse<object>
            {
                Status = false,
                HttpCode = 500,
                Data = null,
                Description = "Failed to assign - could not create ChickenBarn"
            };
            return BadRequest(error);
        }

        var flockUpdateResult = new AssignFlockToLargeChickResponse
        {
            flockId = model.FlockId,
            ChickenLid = chickenLid,
            ChickenLName = largeChicken.Name,
            barnId = model.BarnId,
            CBarnStatus = chickenBarn.Status,
            StartDate = chickenBarn.StartDate
        };

        var success = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = flockUpdateResult,
            Description = "Successfully assigned"
        };
        return Ok(success);
    }
}