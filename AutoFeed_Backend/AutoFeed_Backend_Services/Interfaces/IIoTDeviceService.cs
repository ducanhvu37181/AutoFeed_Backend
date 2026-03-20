namespace AutoFeed_Backend_Services.Interfaces;

public interface IIoTDeviceService
{
    System.Threading.Tasks.Task<IEnumerable<object>> GetAllDevicesAsync(string search, string type, string status);
    System.Threading.Tasks.Task<bool> RegisterDeviceAsync(string name, string description);
    System.Threading.Tasks.Task<bool> UpdateDeviceAsync(int id, string name, string description, bool status);
    System.Threading.Tasks.Task<bool> DeleteDeviceAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<object>> GetBarnsAsync();
    System.Threading.Tasks.Task<bool> ReassignDeviceAsync(int deviceId, int barnId);
}