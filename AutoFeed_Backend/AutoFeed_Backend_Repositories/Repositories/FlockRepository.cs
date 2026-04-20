using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class FlockRepository : GenericRepository<FlockChicken>
{
    public FlockRepository() : base() { }
    public FlockRepository(AutoFeedDBContext context) : base(context) { }

    public new async Task<List<FlockChicken>> GetAllAsync()
    {
        return await _context.FlockChickens
            .Include(f => f.ChickenBarn)
            .ToListAsync();
    }

    public new async Task<FlockChicken?> GetByIdAsync(int id)
    {
        return await _context.FlockChickens
            .Include(f => f.ChickenBarn)
            .FirstOrDefaultAsync(x => x.FlockId == id);
    }

    public async Task<List<FlockChicken>> GetAllWithBarnAsync()
    {
        return await _context.FlockChickens
            .Include(f => f.ChickenBarn)
                .ThenInclude(cb => cb.Barn)
            .ToListAsync();
    }

    public async Task<bool> TransferQuantityToFlockAsync(int sourceFlockId, int targetFlockId)
    {
        try
        {
            var sourceFlock = await _context.FlockChickens
                .FirstOrDefaultAsync(x => x.FlockId == sourceFlockId);
            if (sourceFlock == null) return false;

            var targetFlock = await _context.FlockChickens
                .FirstOrDefaultAsync(x => x.FlockId == targetFlockId);
            if (targetFlock == null) return false;

            // Reduce source flock quantity by 1
            if (sourceFlock.Quantity > 0)
            {
                sourceFlock.Quantity -= 1;
                _context.FlockChickens.Update(sourceFlock);
            }

            // Increase target flock quantity by 1
            targetFlock.Quantity += 1;
            _context.FlockChickens.Update(targetFlock);

            // If source flock is sick, set target flock to sick
            if (sourceFlock.HealthStatus?.ToLower() == "sick")
            {
                targetFlock.HealthStatus = "Sick";
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TransferQuantityBackToFlockAsync(int sourceFlockId, int targetFlockId)
    {
        try
        {
            var sourceFlock = await _context.FlockChickens
                .FirstOrDefaultAsync(x => x.FlockId == sourceFlockId);
            if (sourceFlock == null) return false;

            var targetFlock = await _context.FlockChickens
                .FirstOrDefaultAsync(x => x.FlockId == targetFlockId);
            if (targetFlock == null) return false;

            // Reduce source flock quantity by 1
            if (sourceFlock.Quantity > 0)
            {
                sourceFlock.Quantity -= 1;
                _context.FlockChickens.Update(sourceFlock);
            }

            // Increase target flock quantity by 1
            targetFlock.Quantity += 1;
            _context.FlockChickens.Update(targetFlock);

            // If target flock is healthy, set source flock to healthy (reverse logic)
            if (targetFlock.HealthStatus?.ToLower() == "healthy")
            {
                sourceFlock.HealthStatus = "Healthy";
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TransferQuantityToFlockAndBarnAsync(int sourceFlockId, int targetFlockId, int targetBarnId)
    {
        try
        {
            var sourceFlock = await _context.FlockChickens
                .FirstOrDefaultAsync(x => x.FlockId == sourceFlockId);
            if (sourceFlock == null) return false;

            var targetFlock = await _context.FlockChickens
                .FirstOrDefaultAsync(x => x.FlockId == targetFlockId);
            if (targetFlock == null) return false;

            // Reduce source flock quantity by 1
            if (sourceFlock.Quantity > 0)
            {
                sourceFlock.Quantity -= 1;
                _context.FlockChickens.Update(sourceFlock);
            }

            // Increase target flock quantity by 1
            targetFlock.Quantity += 1;
            _context.FlockChickens.Update(targetFlock);

            // If source flock is sick, set target flock to sick
            if (sourceFlock.HealthStatus?.ToLower() == "sick")
            {
                targetFlock.HealthStatus = "Sick";
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}