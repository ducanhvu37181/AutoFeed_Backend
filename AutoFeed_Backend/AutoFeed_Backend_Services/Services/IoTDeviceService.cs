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
                DeviceID = d.DeviceId, // ID số nguyên chuẩn Database
                DeviceName = d.Name,
                Description = d.Description,
                // Lấy thông tin loại chuồng (Type) từ bảng Barn thông qua bảng trung gian BarnIoTDevices
                AssignedTo = d.BarnIoTDevices.OrderByDescending(b => b.InstallationDate)
                             .FirstOrDefault()?.Barn?.Type ?? "Unassigned",
                Status = d.Status == true ? "Online" : "Offline"
                // Đã loại bỏ hoàn toàn Battery và LastUpdate (Data fake)
            });
        }

        public async Task<bool> RegisterDeviceAsync(string name, string description)
        {
            var device = new IoTDevice
            {
                Name = name,
                Description = description,
                Status = true // Mặc định thiết bị mới sẽ ở trạng thái Online (1)
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
            // Logic gán thiết bị dựa trên DeviceID và BarnID (Sử dụng ID số nguyên)
            // Hàm này sẽ tạo bản ghi mới trong bảng trung gian BarnIoT_Device trong Database
            await _unitOfWork.IoTDevices.ReassignDeviceAsync(deviceId, barnId);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }
    }
}