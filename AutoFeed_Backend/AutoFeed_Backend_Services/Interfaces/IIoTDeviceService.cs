using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces
{
    public interface IIoTDeviceService
    {
        Task<IEnumerable<object>> GetAllDevicesAsync(string search, string type, string status);
        Task<bool> RegisterDeviceAsync(string name, string description);
        Task<bool> UpdateDeviceAsync(int id, string name, string description, bool status);
        Task<bool> DeleteDeviceAsync(int id);
        Task<bool> ReassignDeviceAsync(int deviceId, int barnId);
    }
}