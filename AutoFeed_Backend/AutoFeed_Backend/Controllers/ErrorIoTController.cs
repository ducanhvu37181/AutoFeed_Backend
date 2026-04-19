using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Requests;
using AutoFeed_Backend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorIoTController : ControllerBase
{
    private readonly IErrorIoTService _service;

    public ErrorIoTController(IErrorIoTService service)
    {
        _service = service;
    }

    // POST api/ErrorIoT
    // Add new ErrorIoT record
    [HttpPost]
    public async Task<IActionResult> AddErrorIoT([FromBody] ErrorIoTCreateRequest model)
    {
        if (model == null || model.DeviceId <= 0 || model.BarnId <= 0 || string.IsNullOrWhiteSpace(model.ErrorMessage))
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid request. DeviceId, BarnId, and ErrorMessage are required"
            });
        }

        try
        {
            var errorIoT = await _service.AddErrorIoTAsync(model.DeviceId, model.BarnId, model.ErrorMessage, model.Severity);

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = new
                {
                    ErrorId = errorIoT.ErrorId,
                    DeviceId = errorIoT.DeviceId,
                    BarnId = errorIoT.BarnId,
                    ErrorMessage = errorIoT.ErrorMessage,
                    Severity = errorIoT.Severity,
                    RecordDate = errorIoT.RecordDate?.ToString("yyyy-MM-dd HH:mm:ss")
                },
                Description = "ErrorIoT added successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }

    // GET api/ErrorIoT
    // Get all ErrorIoT records
    [HttpGet]
    public async Task<IActionResult> GetAllErrorIoT()
    {
        try
        {
            var data = await _service.GetAllErrorIoTAsync();

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = data,
                Description = "Success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }

    // GET api/ErrorIoT/{errorId}
    // Get ErrorIoT record by ID (Note: ErrorId is auto-generated, this returns a single error if found)
    [HttpGet("{errorId:int}")]
    public async Task<IActionResult> GetErrorIoTById(int errorId)
    {
        if (errorId <= 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid errorId"
            });
        }

        try
        {
            var data = await _service.GetAllErrorIoTAsync();
            var errorRecord = data.FirstOrDefault(e => 
            {
                dynamic record = e;
                return record.ErrorId == errorId;
            });

            if (errorRecord == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 404,
                    Data = null,
                    Description = "ErrorIoT record not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = errorRecord,
                Description = "Success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }

    // GET api/ErrorIoT/device/{deviceId}
    // Get ErrorIoT records by DeviceId
    [HttpGet("device/{deviceId:int}")]
    public async Task<IActionResult> GetErrorIoTByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Invalid deviceId"
            });
        }

        try
        {
            var data = await _service.GetErrorIoTByDeviceIdAsync(deviceId);

            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = data,
                Description = "Success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = ex.Message
            });
        }
    }
}
