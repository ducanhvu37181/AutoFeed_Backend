using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class ErrorIoTRepository : GenericRepository<ErrorIoT>
{
    public ErrorIoTRepository() : base() { }

    public ErrorIoTRepository(AutoFeedDBContext context) : base(context) { }

    // Get all ErrorIoT with related Device and Barn information
    public async Task<List<ErrorIoT>> GetAllWithDetailsAsync()
    {
        return await _context.ErrorIoTs
            .Include(e => e.Device)
            .Include(e => e.Barn)
            .OrderByDescending(e => e.RecordDate)
            .ToListAsync();
    }

    // Get ErrorIoT records by DeviceId
    public async Task<List<ErrorIoT>> GetByDeviceIdAsync(int deviceId)
    {
        return await _context.ErrorIoTs
            .Include(e => e.Device)
            .Include(e => e.Barn)
            .Where(e => e.DeviceId == deviceId)
            .OrderByDescending(e => e.RecordDate)
            .ToListAsync();
    }
}
