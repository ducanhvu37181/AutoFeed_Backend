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
        var data = await _flockService.GetFlockDashboardAsync(searchTerm, barnId, status);
        return Ok(new { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var data = await _flockService.GetFlockDetailAsync(id);
        return data != null ? Ok(new { Success = true, Data = data }) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FlockChicken flock, [FromQuery] int barnId)
    {
        var res = await _flockService.CreateFlockAsync(flock, barnId);
        return res ? Ok(new { Message = "Created" }) : BadRequest();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] FlockChicken flock)
    {
        var res = await _flockService.UpdateFlockAsync(flock);
        return res ? Ok(new { Message = "Updated" }) : BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _flockService.DeleteFlockAsync(id);
        return res ? Ok(new { Message = "Deleted" }) : NotFound();
    }

    [HttpPost("assign-ids/{id}")]
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

        await _largeChickenService.CreateAsync(largeChicken);
        var chickenLid = ;

        var chickenBarn = new ChickenBarn
        {
            BarnId = model.BarnId,
            ChickenLid = ,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            Note = model.Note,
            Status = "active"
        };

        await _chickenBarnService.CreateAsync(chickenBarn);

        
    }
}