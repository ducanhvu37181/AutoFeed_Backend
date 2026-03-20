using AutoFeed_Backend_DAO.Models;
using Microsoft.AspNetCore.Authorization;
using AutoFeed_Backend_Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public DeviceController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll(string search = "", string type = "All Types", string status = "All Status")
    {
        var devices = await _unitOfWork.IoTDevices.GetDevicesWithBarnAsync(search, type, status);
        var result = devices.Select(d => new {
            DeviceID = "DEV" + d.DeviceId.ToString("D3"),
            DeviceType = d.Name,
            AssignedTo = d.BarnIoTDevices.OrderByDescending(b => b.InstallationDate).FirstOrDefault()?.Barn?.Type ?? "Unassigned",
            Status = d.Status == true ? "Online" : "Offline",
            Battery = "85%",
            LastUpdate = "2 min ago"
        });
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string name, string description)
    {
        var device = new IoTDevice
        {
            Name = name,
            Description = description,
            Status = true
        };
        await _unitOfWork.IoTDevices.CreateAsync(device);
        return Ok(new { message = "Success" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, string name, string description, bool status)
    {
        var device = await _unitOfWork.IoTDevices.GetByIdAsync(id);
        if (device == null) return NotFound();

        device.Name = name;
        device.Description = description;
        device.Status = status;

        await _unitOfWork.IoTDevices.UpdateAsync(device);
        return Ok(new { message = "Success" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var device = await _unitOfWork.IoTDevices.GetByIdAsync(id);
        if (device == null) return NotFound();
        await _unitOfWork.IoTDevices.RemoveAsync(device);
        return Ok(new { message = "Success" });
    }

    [HttpGet("barns")]
    public async Task<IActionResult> GetBarns()
    {
        var barns = await _unitOfWork.IoTDevices.GetAllBarnsAsync();
        return Ok(barns.Select(b => new { b.BarnId, b.Type }));
    }

    [HttpPost("reassign")]
    public async Task<IActionResult> Reassign(int deviceId, int barnId)
    {
        await _unitOfWork.IoTDevices.ReassignDeviceAsync(deviceId, barnId);
        return Ok(new { message = "Update Assignment Success!" });
    }
}