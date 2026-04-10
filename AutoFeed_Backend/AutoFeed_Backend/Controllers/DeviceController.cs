using Microsoft.AspNetCore.Mvc;
using AutoFeed_Backend.Models.Requests.Device;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Models.Responses;

namespace AutoFeed_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTDeviceController : ControllerBase
    {
        private readonly IIoTDeviceService _deviceService;

        public IoTDeviceController(IIoTDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? search, string? type, string? status)
        {
            var result = await _deviceService.GetAllDevicesAsync(search ?? "", type ?? "", status ?? "");
            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = result,
                Description = "Devices retrieved successfully."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DeviceCreateRequest request)
        {
            var success = await _deviceService.RegisterDeviceAsync(request.Name, request.Description);
            if (success)
                return Ok(new ApiResponse<object>
                {
                    Status = true,
                    HttpCode = 200,
                    Data = null,
                    Description = "Device registered successfully!"
                });

            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Failed to register device."
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DeviceUpdateRequest request)
        {
            if (id != request.DeviceID)
                return BadRequest(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 400,
                    Data = null,
                    Description = "ID mismatch!"
                });

            var success = await _deviceService.UpdateDeviceAsync(id, request.Name, request.Description, request.Status);
            if (success)
                return Ok(new ApiResponse<object>
                {
                    Status = true,
                    HttpCode = 200,
                    Data = null,
                    Description = "Device updated successfully!"
                });

            return NotFound(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Device not found."
            });
        }

        [HttpPut("{id}/assign-barn")]
        public async Task<IActionResult> AssignToBarn(int id, [FromBody] DeviceAssignRequest request)
        {
            var success = await _deviceService.ReassignDeviceAsync(id, request.BarnID);
            if (success)
                return Ok(new ApiResponse<object>
                {
                    Status = true,
                    HttpCode = 200,
                    Data = null,
                    Description = $"Device {id} assigned to barn {request.BarnID} successfully!"
                });

            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = "Assignment failed."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _deviceService.DeleteDeviceAsync(id);
            if (success)
                return Ok(new ApiResponse<object>
                {
                    Status = true,
                    HttpCode = 200,
                    Data = null,
                    Description = "Device deleted successfully!"
                });

            return NotFound(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 404,
                Data = null,
                Description = "Device not found for deletion."
            });
        }
    }
}