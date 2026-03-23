using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ScheduleModel = AutoFeed_Backend_DAO.Models.Schedule;
using AutoFeed_Backend.Models.Requests;
using AutoFeed_Backend.Models.Requests.Schedule;
using AutoFeed_Backend.Models.Responses;
using System.Linq;
using System.Collections.Generic;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _service;

    public ScheduleController(IScheduleService service)
    {
        _service = service;
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var items = await _service.GetSchedulesByUserAsync(userId);
        var user = await _service.GetUserNameByIdAsync(userId);
        var dto = items.Select(s => ToDto(s, user)).ToList();
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    private ScheduleResponse ToDto(ScheduleModel s, string username = null) => new ScheduleResponse
    {
        SchedId = s.SchedId,
        UserId = s.UserId,
        TaskId = s.TaskId,
        CbarnId = s.CbarnId,
        Description = s.Description,
        Note = s.Note,
        Priority = s.Priority,
        Status = s.Status,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Username = username
    };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllSchedulesAsync();
        // fetch usernames for user ids
        var users = items.Where(i => i.UserId.HasValue).Select(i => i.UserId.Value).Distinct().ToList();
        var userMap = new Dictionary<int, string>();
        foreach (var uid in users)
        {
            var u = await _service.GetUserNameByIdAsync(uid);
            userMap[uid] = u;
        }
        var dto = items.Select(s => ToDto(s, s.UserId.HasValue && userMap.ContainsKey(s.UserId.Value) ? userMap[s.UserId.Value] : null)).ToList();
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet("inprogress")]
    public async Task<IActionResult> GetInProgress()
    {
        var items = await _service.GetInProgressScheduleAsync();
        var users = items.Where(i => i.UserId.HasValue).Select(i => i.UserId.Value).Distinct().ToList();
        var userMap = new Dictionary<int, string>();
        foreach (var uid in users)
        {
            var u = await _service.GetUserNameByIdAsync(uid);
            userMap[uid] = u;
        }
        var dto = items.Select(s => ToDto(s, s.UserId.HasValue && userMap.ContainsKey(s.UserId.Value) ? userMap[s.UserId.Value] : null)).ToList();
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompleted()
    {
        var items = await _service.GetCompletedScheduleAsync();
        var users = items.Where(i => i.UserId.HasValue).Select(i => i.UserId.Value).Distinct().ToList();
        var userMap = new Dictionary<int, string>();
        foreach (var uid in users)
        {
            var u = await _service.GetUserNameByIdAsync(uid);
            userMap[uid] = u;
        }
        var dto = items.Select(s => ToDto(s, s.UserId.HasValue && userMap.ContainsKey(s.UserId.Value) ? userMap[s.UserId.Value] : null)).ToList();
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
        var items = await _service.SearchSchedulesAsync(q);
        var users = items.Where(i => i.UserId.HasValue).Select(i => i.UserId.Value).Distinct().ToList();
        var userMap = new Dictionary<int, string>();
        foreach (var uid in users)
        {
            var u = await _service.GetUserNameByIdAsync(uid);
            userMap[uid] = u;
        }
        var dto = items.Select(s => ToDto(s, s.UserId.HasValue && userMap.ContainsKey(s.UserId.Value) ? userMap[s.UserId.Value] : null)).ToList();
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
        var item = await _service.GetScheduleByIdAsync(id);
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
        var username = item.UserId.HasValue ? await _service.GetUserNameByIdAsync(item.UserId.Value) : null;
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = ToDto(item, username),
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScheduleRequest model)
    {
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

        var schedule = new ScheduleModel
        {
            UserId = model.UserId,
            TaskId = model.TaskId,
            CbarnId = model.CbarnId,
            Description = model.Description,
            Note = model.Note,
            Priority = model.Priority,
            Status = model.Status ?? "pending",
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        var id = await _service.CreateScheduleAsync(schedule);
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

        var username = schedule.UserId.HasValue ? await _service.GetUserNameByIdAsync(schedule.UserId.Value) : null;
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 201,
            Data = ToDto(schedule, username),
            Description = "Created"
        };
        return CreatedAtAction(nameof(Get), new { id = schedule.SchedId }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateScheduleRequest model)
    {
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

        var existing = await _service.GetScheduleByIdAsync(id);
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

        existing.UserId = model.UserId;
        existing.TaskId = model.TaskId;
        existing.CbarnId = model.CbarnId;
        existing.Description = model.Description;
        existing.Note = model.Note;
        existing.Priority = model.Priority;
        if (!string.IsNullOrWhiteSpace(model.Status)) existing.Status = model.Status;
        existing.StartDate = model.StartDate;
        existing.EndDate = model.EndDate;

        var ok = await _service.UpdateScheduleAsync(existing);
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

        var username = existing.UserId.HasValue ? await _service.GetUserNameByIdAsync(existing.UserId.Value) : null;
        var updated = ToDto(existing, username);
        var successResponse = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = updated,
            Description = "Update success"
        };
        return Ok(successResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteScheduleAsync(id);
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
