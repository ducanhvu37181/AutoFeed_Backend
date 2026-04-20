using AutoFeed_Backend.Models.Requests;
using AutoFeed_Backend.Models.Responses;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataIoTController : ControllerBase
{
    private readonly IDataIoTService _service;

    public DataIoTController(IDataIoTService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddDataIoTRequest req)
    {
        if (req == null) return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Description = "Invalid request" });

        var res = await _service.AddDataAsync(req.BarnId, req.DeviceId, req.Value, req.Description);
        if (res == null) return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Description = "Failed to add data" });

        var response = new AddDataIoTResponse
        {
            BarnId = res.Value.BarnId,
            DeviceName = res.Value.DeviceName,
            Value = res.Value.Value,
            Description = res.Value.Description,
            RecordDate = res.Value.RecordDate,
            SequenceNumber = res.Value.SequenceNumber
        };

        return Ok(new ApiResponse<AddDataIoTResponse> { Status = true, HttpCode = 200, Data = response, Description = "Add data successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        var resp = new System.Collections.Generic.List<AddDataIoTResponse>();
        foreach (var d in list)
        {
            resp.Add(new AddDataIoTResponse
            {
                BarnId = d.BarnId,
                DeviceName = d.DeviceName,
                Value = d.Value,
                Description = d.Description,
                RecordDate = d.RecordDate,
                SequenceNumber = d.SequenceNumber
            });
        }

        return Ok(new ApiResponse<System.Collections.Generic.List<AddDataIoTResponse>> { Status = true, HttpCode = 200, Data = resp, Description = "Get all data successfully" });
    }

    [HttpGet("by-description")]
    public async Task<IActionResult> GetByDescription([FromQuery] string description)
    {
        if (string.IsNullOrEmpty(description)) return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Description = "Description is required" });

        var list = await _service.GetByDescriptionAsync(description);
        var resp = new System.Collections.Generic.List<AddDataIoTResponse>();
        foreach (var d in list)
        {
            resp.Add(new AddDataIoTResponse
            {
                BarnId = d.BarnId,
                DeviceName = d.DeviceName,
                Value = d.Value,
                Description = d.Description,
                RecordDate = d.RecordDate,
                SequenceNumber = d.SequenceNumber
            });
        }

        return Ok(new ApiResponse<System.Collections.Generic.List<AddDataIoTResponse>> { Status = true, HttpCode = 200, Data = resp, Description = "Get data by description successfully" });
    }

    [HttpGet("by-device/{deviceId}")]
    public async Task<IActionResult> GetByDeviceId(int deviceId)
    {
        if (deviceId <= 0) return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Description = "Invalid deviceId" });

        var list = await _service.GetByDeviceIdAsync(deviceId);
        var resp = new System.Collections.Generic.List<AddDataIoTResponse>();
        foreach (var d in list)
        {
            resp.Add(new AddDataIoTResponse
            {
                BarnId = d.BarnId,
                DeviceName = d.DeviceName,
                Value = d.Value,
                Description = d.Description,
                RecordDate = d.RecordDate,
                SequenceNumber = d.SequenceNumber
            });
        }

        return Ok(new ApiResponse<System.Collections.Generic.List<AddDataIoTResponse>> { Status = true, HttpCode = 200, Data = resp, Description = "Get data by deviceId successfully" });
    }

    [HttpGet("by-barn/{barnId}")]
    public async Task<IActionResult> GetByBarnId(int barnId)
    {
        if (barnId <= 0) return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Description = "Invalid barnId" });

        var list = await _service.GetByBarnIdAsync(barnId);
        var resp = new System.Collections.Generic.List<AddDataIoTResponse>();
        foreach (var d in list)
        {
            resp.Add(new AddDataIoTResponse
            {
                BarnId = d.BarnId,
                DeviceName = d.DeviceName,
                Value = d.Value,
                Description = d.Description,
                RecordDate = d.RecordDate,
                SequenceNumber = d.SequenceNumber
            });
        }

        return Ok(new ApiResponse<System.Collections.Generic.List<AddDataIoTResponse>> { Status = true, HttpCode = 200, Data = resp, Description = "Get data by barnId successfully" });
    }

    [HttpDelete("by-day")]
    public async Task<IActionResult> WipeByDay([FromQuery] DateTime date)
    {
        if (date == default) return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Description = "Invalid date" });

        var r = await _service.RemoveByDateAsync(date);
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = new { Deleted = r }, Description = "Wipe data by day successfully" });
    }
}
