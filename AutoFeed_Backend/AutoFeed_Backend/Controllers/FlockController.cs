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

    public FlockController(IFlockService flockService)
    {
        _flockService = flockService;
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
    public async Task<IActionResult> AssignIds(int id)
    {
        var res = await _flockService.AssignIdsToLargeChickensAsync(id);
        return res ? Ok(new { Message = "Assigned" }) : BadRequest();
    }
}