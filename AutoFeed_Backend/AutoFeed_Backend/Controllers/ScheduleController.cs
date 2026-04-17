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
        var dto = await _service.GetSchedulesByUserResponsesAsync(userId);
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet("user/{userId:int}/date")]
    public async Task<IActionResult> GetByUserAndDate(int userId, [FromQuery] System.DateTime date)
    {
        var dto = await _service.GetSchedulesByUserAndDateAsync(userId, date);
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = dto,
            Description = "Success"
        };
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var dto = await _service.GetAllScheduleResponsesAsync();
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
        var dto = await _service.GetInProgressScheduleResponsesAsync();
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
        var dto = await _service.GetCompletedScheduleResponsesAsync();
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
        var dto = await _service.SearchScheduleResponsesAsync(q);
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
        var dto = await _service.GetScheduleResponseByIdAsync(id);
        if (dto == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
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

        var id = await _service.CreateMultipleScheduleAsync(schedule);
        if (id == -1)
        {
            return StatusCode(409, new ApiResponse<object>
            {
                Status = false,
                HttpCode = 409,
                Data = null,
                Description = "Schedule conflict: overlapping schedule exists"
            });
        }

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

        var responseDto = await _service.GetScheduleResponseByIdAsync(id);
        var response = new ApiResponse<object>
        {
            Status = true,
            HttpCode = 201,
            Data = responseDto,
            Description = "Created"
        };
        return CreatedAtAction(nameof(Get), new { id = schedule.SchedId }, response);
    }

    // New endpoint: create multiple schedules for each day in range
    [HttpPost("multiple")]
    public async Task<IActionResult> CreateMultiple([FromBody] CreateScheduleRequest model)
    {
        if (model == null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request"
            });
        }

        if (!model.EndDate.HasValue)
        {
            // No end date provided — treat as single create
            return await Create(model);
        }

        if (model.EndDate.Value < model.StartDate)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "EndDate cannot be earlier than StartDate"
            });
        }

        var end = model.EndDate.Value;

        // build schedules for each date in range
        var schedules = new List<ScheduleModel>();
        for (var date = model.StartDate; date <= end; date = date.AddDays(1))
        {
            schedules.Add(new ScheduleModel
            {
                UserId = model.UserId,
                TaskId = model.TaskId,
                CbarnId = model.CbarnId,
                Description = model.Description,
                Note = model.Note,
                Priority = model.Priority,
                Status = model.Status ?? "pending",
                StartDate = date,
                EndDate = date
            });
        }

        var results = await _service.CreateSingleScheduleAsync(schedules);

        var responseItems = new List<object>();
        for (int i = 0; i < results.Count; i++)
        {
            var res = results[i];
            var sched = schedules[i];
            object item;
            if (res.Success && res.SchedId.HasValue)
            {
                var dto = await _service.GetScheduleResponseByIdAsync(res.SchedId.Value);
                item = new { Date = sched.StartDate, Success = true, Message = res.Message, Schedule = dto };
            }
            else
            {
                item = new { Date = sched.StartDate, Success = false, Message = res.Message };
            }
            responseItems.Add(item);
        }

        // if any failed, return 207 Multi-Status semantics; otherwise 201
        var anyFailed = results.Any(r => !r.Success);
        return StatusCode(anyFailed ? 207 : 201, new ApiResponse<object>
        {
            Status = !anyFailed,
            HttpCode = anyFailed ? 207 : 201,
            Data = responseItems,
            Description = anyFailed ? "Partial success" : "Created"
        });
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

        existing.UserId = model.UserId; // No change
        existing.TaskId = model.TaskId; // No change
        existing.CbarnId = model.CbarnId; // No change
        existing.Description = model.Description;
        existing.Note = model.Note;
        existing.Priority = model.Priority;
        if (!string.IsNullOrWhiteSpace(model.Status)) existing.Status = model.Status;
        existing.StartDate = model.StartDate; // No change
        existing.EndDate = model.EndDate; // No change

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

        var updated = await _service.GetScheduleResponseByIdAsync(id);
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

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid status" });

        var ok = await _service.UpdateScheduleStatusAsync(id, status);
        if (!ok)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found or Update failed" });

        var updated = await _service.GetScheduleResponseByIdAsync(id);
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = updated, Description = "Status updated" });
    }
}
