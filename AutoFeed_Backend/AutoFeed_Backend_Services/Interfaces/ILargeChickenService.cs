using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface ILargeChickenService
{
    // Read
    Task<List<LargeChicken>> GetAllAsync();
    Task<List<LargeChicken>> GetActiveAsync();
    Task<List<LargeChicken>> GetInactiveAsync();
    Task<LargeChicken?> GetByIdAsync(int id);
    Task<List<LargeChicken>> SearchAsync(string? name, string? healthStatus, int? flockId, bool includeInactive);

    // Create
    Task<int> CreateAsync(LargeChicken entity);

    // Update
    Task<bool> UpdateAsync(LargeChicken entity);

    // Soft delete / Restore
    Task<bool> DeleteAsync(int id);   // isActive = false
    Task<bool> RestoreAsync(int id);  // isActive = true
}