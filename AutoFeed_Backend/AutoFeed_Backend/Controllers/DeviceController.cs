using Microsoft.AspNetCore.Mvc;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Requests;  // Dùng Model từ Project Service
using AutoFeed_Backend_Services.Models.Responses; // Dùng Model từ Project Service
using AutoFeed_Backend.Models.Responses;          // Dùng ApiResponse chung của Web

namespace AutoFeed_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTDeviceController : ControllerBase
    {
        private readonly IIoTDeviceService _deviceService;

        // Chỉ tiêm Service vào đây, không dùng UnitOfWork ở Controller nữa
        public IoTDeviceController(IIoTDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        // 1b. Get devices by barn id
        [HttpGet("barn/{barnId}")]
        public async Task<IActionResult> GetByBarn(int barnId)
        {
            var result = await _deviceService.GetDevicesByBarnIdAsync(barnId);
            return Ok(new ApiResponse<object>
            {
                Status = true,
                HttpCode = 200,
                Data = result,
                Description = $"Devices for barn {barnId} retrieved successfully."
            });
        }

        // 1c. Get barn assigned to a device (latest assignment)
        [HttpGet("{deviceId}/barn")]
        public async Task<IActionResult> GetBarnByDevice(int deviceId)
        {
            var result = await _deviceService.GetBarnByDeviceIdAsync(deviceId);
            if (result == null)
                return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Barn not found for device." });

            return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = result, Description = "Barn retrieved successfully." });
        }

        // 1. Lấy danh sách tất cả thiết bị
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

        // 2. Lấy chi tiết thiết bị theo ID (Hàm mới Kiên yêu cầu)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _deviceService.GetDeviceByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 404,
                    Data = null,
                    Description = $"Device with ID {id} not found."
                });
            }

            return Ok(new ApiResponse<DeviceDetailResponse>
            {
                Status = true,
                HttpCode = 200,
                Data = result,
                Description = "Device details retrieved successfully."
            });
        }

        // 3. Đăng ký thiết bị mới
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

        // 4. Cập nhật thông tin thiết bị
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

        // 5. Gán thiết bị vào chuồng
        [HttpPut("{id}/assign-barn")]
        public async Task<IActionResult> AssignToBarn(int id, [FromBody] DeviceAssignRequest request)
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device == null)
                return NotFound(new ApiResponse<object>
                {
                    Status = false,
                    HttpCode = 404,
                    Data = null,
                    Description = "Device not found."
                });

            var success = await _deviceService.ReassignDeviceAsync(id, request.BarnID);
            if (success)
                return Ok(new ApiResponse<object>
                {
                    Status = true,
                    HttpCode = 200,
                    Data = null,
                    Description = $"Device {id} assigned successfully!"
                });

            return BadRequest(new ApiResponse<object>
            {
                Status = false,
                HttpCode = 400,
                Data = null,
                Description = $"Assignment failed. A device with the name '{device.DeviceName}' is already assigned to this barn."
            });
        }

        // 6. Xóa thiết bị
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