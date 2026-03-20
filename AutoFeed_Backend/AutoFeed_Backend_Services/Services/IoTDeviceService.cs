using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;

namespace AutoFeed_Backend_Services.Services;

public class IoTDeviceService : IIoTDeviceService
{
    private readonly IUnitOfWork _unitOfWork;

    public IoTDeviceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<IEnumerable<object>> GetAllDevicesAsync(string search, string type, string status)
    {
        var devices = await _unitOfWork.IoTDevices.GetDevicesWithBarnAsync(search, type, status);
        return devices.Select(d => new {
            DeviceID = "DEV" + d.DeviceId.ToString("D3"),
            DeviceType = d.Name,
            AssignedTo = d.BarnIoTDevices.OrderByDescending(b => b.InstallationDate).FirstOrDefault()?.Barn?.Type ?? "Unassigned",
            Status = d.Status == true ? "Online" : "Offline",
            Battery = "85%",
            LastUpdate = "2 min ago"
        });
    }

    public async System.Threading.Tasks.Task<bool> RegisterDeviceAsync(string name, string description)
    {
        var device = new IoTDevice { Name = name, Description = description, Status = true };
        await _unitOfWork.IoTDevices.CreateAsync(device);
        return true;
    }

    public async System.Threading.Tasks.Task<bool> UpdateDeviceAsync(int id, string name, string description, bool status)
    {
        var device = await _unitOfWork.IoTDevices.GetByIdAsync(id);
        if (device == null) return false;
        device.Name = name;
        device.Description = description;
        device.Status = status;
        await _unitOfWork.IoTDevices.UpdateAsync(device);
        return true;
    }

    public async System.Threading.Tasks.Task<bool> DeleteDeviceAsync(int id)
    {
        var device = await _unitOfWork.IoTDevices.GetByIdAsync(id);
        if (device == null) return false;
        await _unitOfWork.IoTDevices.RemoveAsync(device);
        return true;
    }

    public async System.Threading.Tasks.Task<IEnumerable<object>> GetBarnsAsync()
    {
        var barns = await _unitOfWork.IoTDevices.GetAllBarnsAsync();
        return barns.Select(b => new { b.BarnId, b.Type });
    }

    public async System.Threading.Tasks.Task<bool> ReassignDeviceAsync(int deviceId, int barnId)
    {
        await _unitOfWork.IoTDevices.ReassignDeviceAsync(deviceId, barnId);
        return true;
    }
}