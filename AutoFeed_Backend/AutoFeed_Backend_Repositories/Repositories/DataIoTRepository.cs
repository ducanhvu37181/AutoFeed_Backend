using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class DataIoTRepository : GenericRepository<DataIoT>
{
    public DataIoTRepository() : base() { }
    public DataIoTRepository(AutoFeedDBContext context) : base(context) { }

    public async Task<int> CreateAsync(DataIoT entity)
    {
        _context.DataIoTs.Add(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextSequenceNumberAsync(int deviceId, DateTime? date, string? description)
    {
        var d = date?.Date ?? DateTime.Now.Date;
        var desc = description ?? string.Empty;
        var next = await _context.DataIoTs
            .Where(x => x.DeviceId == deviceId
                        && x.RecordDate.HasValue && x.RecordDate.Value.Date == d
                        && ((x.Description ?? string.Empty) == desc))
            .MaxAsync(x => (int?)x.SequenceNumber);
        return (next ?? 0) + 1;
    }

    public async Task<List<DataIoT>> GetAllAsync()
    {
        return await _context.DataIoTs
            .Include(d => d.Device)
            .ToListAsync();
    }

    public async Task<List<DataIoT>> GetByDescriptionAsync(string description)
    {
        return await _context.DataIoTs
            .Include(d => d.Device)
            .Where(x => (x.Description ?? string.Empty) == description)
            .ToListAsync();
    }

    public async Task<List<DataIoT>> GetByDeviceIdAsync(int deviceId)
    {
        return await _context.DataIoTs
            .Include(d => d.Device)
            .Where(x => x.DeviceId == deviceId)
            .ToListAsync();
    }

    public async Task<List<DataIoT>> GetByBarnIdAsync(int barnId)
    {
        return await _context.DataIoTs
            .Include(d => d.Device)
            .Where(x => x.BarnId == barnId)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalFoodByDateRangeAsync(int barnId, DateTime startDate, DateTime endDate)
    {
        var data = await _context.DataIoTs
            .Where(x => x.BarnId == barnId
                        && x.Description == "food today"
                        && x.RecordDate.HasValue
                        && x.RecordDate.Value.Date >= startDate.Date
                        && x.RecordDate.Value.Date <= endDate.Date)
            .ToListAsync();

        if (data.Count == 0) return 0;

        var total = data
            .GroupBy(x => x.RecordDate!.Value.Date)
            .Sum(g => g.OrderByDescending(x => x.SequenceNumber).First().Value);

        return total;
    }

    public async Task<decimal> GetFoodTodayAsync(int barnId)
    {
        var today = DateTime.Now.Date;
        var last = await _context.DataIoTs
            .Where(x => x.BarnId == barnId
                        && x.Description == "food today"
                        && x.RecordDate.HasValue
                        && x.RecordDate.Value.Date == today)
            .OrderByDescending(x => x.SequenceNumber)
            .FirstOrDefaultAsync();

        return last?.Value ?? 0;
    }

    public async Task<int> RemoveByDateAsync(DateTime date)
    {
        var d = date.Date;
        var items = _context.DataIoTs.Where(x => x.RecordDate.HasValue && x.RecordDate.Value.Date == d);
        var count = await items.CountAsync();
        if (count == 0) return 0;
        _context.DataIoTs.RemoveRange(items);
        return await _context.SaveChangesAsync();
    }
}
