using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskModel = AutoFeed_Backend_DAO.Models.Task;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _service;

    public TaskController(ITaskService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllTasksAsync();
        return Ok(items);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var items = await _service.GetActiveTasksAsync();
        return Ok(items);
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive()
    {
        var items = await _service.GetInactiveTasksAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetTaskByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskModel model)
    {
        var id = await _service.CreateTaskAsync(model);
        return CreatedAtAction(nameof(Get), new { id = model.TaskId }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskModel model)
    {
        if (id != model.TaskId) return BadRequest();
        var ok = await _service.UpdateTaskAsync(model);
        if (!ok) return BadRequest();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteTaskAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
