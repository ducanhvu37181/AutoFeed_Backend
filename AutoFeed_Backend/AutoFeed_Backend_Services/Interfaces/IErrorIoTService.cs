using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IErrorIoTService
{
    // Add new ErrorIoT record
    Task<ErrorIoT> AddErrorIoTAsync(int deviceId, int barnId, string errorMessage, string severity);

    // Get all ErrorIoT records
    Task<IEnumerable<object>> GetAllErrorIoTAsync();

    // Get ErrorIoT records by DeviceId
    Task<IEnumerable<object>> GetErrorIoTByDeviceIdAsync(int deviceId);
}
