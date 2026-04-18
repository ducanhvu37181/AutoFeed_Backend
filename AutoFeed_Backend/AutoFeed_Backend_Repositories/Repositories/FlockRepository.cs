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
            .Include(f => f.ChickenBarns)
            .ToListAsync();
    }

    public new async Task<FlockChicken?> GetByIdAsync(int id)
    {
        return await _context.FlockChickens
            .Include(f => f.ChickenBarns)
            .FirstOrDefaultAsync(x => x.FlockId == id);
    }

    public async Task<List<FlockChicken>> GetAllWithBarnAsync()
    {
        return await _context.FlockChickens
            .Include(f => f.ChickenBarns)
                .ThenInclude(cb => cb.Barn)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<FlockChicken?> TransferFlockToBarnAsync(int flockId, int newBarnId)
    {
        try
        {
            // Get the flock
            var flock = await _context.FlockChickens
                .Include(f => f.ChickenBarns)
                .FirstOrDefaultAsync(x => x.FlockId == flockId);
            if (flock == null)
            {
                Console.WriteLine($"Flock {flockId} not found");
                return null;
            }

            // Check if target barn is empty (no active assignments)
            var targetBarnAssignment = await _context.Set<ChickenBarn>()
                .FirstOrDefaultAsync(cb => cb.BarnId == newBarnId && cb.Status == "active");
            if (targetBarnAssignment != null)
            {
                Console.WriteLine($"Barn {newBarnId} is not empty");
                return null; // Barn is not empty
            }

            // Reduce quantity of source flock by 1
            if (flock.Quantity > 0)
            {
                flock.Quantity -= 1;
                _context.FlockChickens.Update(flock);
                Console.WriteLine($"Reduced flock {flockId} quantity to {flock.Quantity}");
            }

            // Create new assignment to the target barn (don't remove old assignment)
            var newAssignment = new ChickenBarn
            {
                BarnId = newBarnId,
                ChickenLid = null,
                FlockId = flockId,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "active"
            };
            _context.Set<ChickenBarn>().Add(newAssignment);
            Console.WriteLine($"Created new assignment for flock {flockId} to barn {newBarnId}");

            await _context.SaveChangesAsync();
            Console.WriteLine($"Transfer successful for flock {flockId}");

            return flock;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Transfer failed: {ex.Message}");
            return null;
        }
    }
}