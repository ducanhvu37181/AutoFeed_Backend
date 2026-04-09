using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services
{
    public class IoTDeviceService : IIoTDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IoTDeviceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<object>> GetAllDevicesAsync(string search, string type, string status)
        {
            // Gọi Repository lấy dữ liệu kèm bảng liên kết Barn
            var devices = await _unitOfWork.IoTDevices.GetDevicesWithBarnAsync(search, type, status);

            return devices.Select(d => new {
                DeviceID = d.DeviceId, // Trả về số nguyên 1, 2, 3...
                DeviceName = d.Name,
                Description = d.Description,
                // Lấy tên chuồng mới nhất được lắp đặt
                AssignedTo = d.BarnIoTDevices.OrderByDescending(b => b.InstallationDate)
                             .FirstOrDefault()?.Barn?.Type ?? "Unassigned",
                Status = d.Status == true ? "Online" : "Offline",
                Battery = "85%",
                LastUpdate = "2 min ago"
            });
        }

        public async Task<bool> RegisterDeviceAsync(string name, string description)
        {
            var device = new IoTDevice
            {
                Name = name,
                Description = description,
                Status = true // Mặc định máy mới là Online
            };

            await _unitOfWork.IoTDevices.CreateAsync(device);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }

        public async Task<bool> UpdateDeviceAsync(int id, string name, string description, bool status)
        {
            var device = await _unitOfWork.IoTDevices.GetByIdAsync(id);
            if (device == null) return false;

            device.Name = name;
            device.Description = description;
            device.Status = status;

            await _unitOfWork.IoTDevices.UpdateAsync(device);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }

        public async Task<bool> DeleteDeviceAsync(int id)
        {
            var device = await _unitOfWork.IoTDevices.GetByIdAsync(id);
            if (device == null) return false;

            await _unitOfWork.IoTDevices.RemoveAsync(device);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }

        public async Task<bool> ReassignDeviceAsync(int deviceId, int barnId)
        {
            // Gọi hàm xử lý logic gán chuồng trong Repository
            await _unitOfWork.IoTDevices.ReassignDeviceAsync(deviceId, barnId);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }
    }
}