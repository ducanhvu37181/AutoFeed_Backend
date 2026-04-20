using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;

namespace AutoFeed_Backend_Repositories.Repositories;

public class IoTDeviceRepository : GenericRepository<IoTDevice>
{
    public IoTDeviceRepository() : base() { }
    public IoTDeviceRepository(AutoFeedDBContext context) : base(context) { }

    public async System.Threading.Tasks.Task<IEnumerable<IoTDevice>> GetDevicesWithBarnAsync(string search, string type, string status)
    {
        var query = _context.IoTDevices
            .Include(d => d.BarnIoTDevices)
                .ThenInclude(b => b.Barn)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(d => d.Name.Contains(search) || d.DeviceId.ToString().Contains(search));

        if (!string.IsNullOrEmpty(type) && type != "All Types")
            query = query.Where(d => d.Name.Contains(type));

        return await query.ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Barn>> GetAllBarnsAsync() => await _context.Barns.ToListAsync();

    public async System.Threading.Tasks.Task ReassignDeviceAsync(int deviceId, int newBarnId)
    {
        // Get the device being assigned
        var device = await _context.IoTDevices.FindAsync(deviceId);
        if (device == null)
            throw new InvalidOperationException($"Device with ID {deviceId} not found");

        // Check if another device with the same name is already assigned to this barn
        var existingAssignment = await _context.BarnIoTDevices
            .Include(b => b.Device)
            .Where(b => b.BarnId == newBarnId && b.Device.Name == device.Name && b.DeviceId != deviceId)
            .FirstOrDefaultAsync();

        if (existingAssignment != null)
            throw new InvalidOperationException($"A device with the name '{device.Name}' is already assigned to this barn");

        var oldAssignments = _context.BarnIoTDevices.Where(b => b.DeviceId == deviceId);
        _context.BarnIoTDevices.RemoveRange(oldAssignments);

        var newAssignment = new BarnIoTDevice
        {
            DeviceId = deviceId,
            BarnId = newBarnId,
            InstallationDate = DateOnly.FromDateTime(DateTime.Now),
            Status = true
        };
        _context.BarnIoTDevices.Add(newAssignment);

        // Set device status to online when assigned to barn
        device.Status = true;
        _context.Update(device);
    }

    public async System.Threading.Tasks.Task UnassignDeviceAsync(int deviceId)
    {
        var device = await _context.IoTDevices.FindAsync(deviceId);
        var assignments = _context.BarnIoTDevices.Where(b => b.DeviceId == deviceId);
        _context.BarnIoTDevices.RemoveRange(assignments);

        // Set device status to offline when unassigned from barn
        if (device != null)
        {
            device.Status = false;
            _context.Update(device);
        }
    }

    // Get devices assigned to a specific barn including device info and installation date
    public async System.Threading.Tasks.Task<IEnumerable<BarnIoTDevice>> GetDevicesAssignedToBarnAsync(int barnId)
    {
        return await _context.BarnIoTDevices
            .Include(b => b.Device)
            .Where(b => b.BarnId == barnId)
            .OrderByDescending(b => b.InstallationDate)
            .ToListAsync();
    }

    // Get the latest barn assigned to a device (by installation date)
    public async System.Threading.Tasks.Task<Barn?> GetBarnByDeviceIdAsync(int deviceId)
    {
        var assignment = await _context.BarnIoTDevices
            .Include(b => b.Barn)
            .Where(b => b.DeviceId == deviceId)
            .OrderByDescending(b => b.InstallationDate)
            .FirstOrDefaultAsync();

        return assignment?.Barn;
    }
}