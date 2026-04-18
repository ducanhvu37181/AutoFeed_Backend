using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Repositories.UnitOfWork;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoFeed_Backend_DAO.Models;
using ChickenBarnModel = AutoFeed_Backend_DAO.Models.ChickenBarn;

namespace AutoFeed_Backend_Services.Services;

public class ChickenBarnService : IChickenBarnService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChickenBarnService()
    {
        _unitOfWork = new UnitOfWork();
    }

    public ChickenBarnService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateAsync(ChickenBarnModel entity)
    {
        _unitOfWork.ChickenBarns.PrepareCreate(entity);
        await _unitOfWork.SaveChangesWithTransactionAsync();
        return entity.CbarnId;  // Return the actual ID, not the row count
    }

    public async Task<ChickenBarnModel?> GetByIdAsync(int id)
    {
        return await _unitOfWork.ChickenBarns.GetByIdAsync(id);
    }

    public async Task<List<ChickenBarnModel>> GetAllAsync()
    {
        return await _unitOfWork.ChickenBarns.GetAllAsync();
    }

    public async Task<List<ChickenBarnModel>> GetActiveAsync()
    {
        return await _unitOfWork.ChickenBarns.GetActiveAsync();
    }

    public async Task<List<ChickenBarnModel>> GetExportedAsync()
    {
        return await _unitOfWork.ChickenBarns.GetExportedAsync();
    }

    public async Task<bool> UpdateAsync(ChickenBarnModel entity)
    {
        try
        {
            _unitOfWork.ChickenBarns.PrepareUpdate(entity);
            var r = await _unitOfWork.SaveChangesWithTransactionAsync();
            return r > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.ChickenBarns.GetByIdAsync(id);
        if (entity == null) return false;
        entity.Status = "inactive";
        _unitOfWork.ChickenBarns.PrepareUpdate(entity);
        var r = await _unitOfWork.SaveChangesWithTransactionAsync();
        return r > 0;
    }


    public async Task<ChickenBarnModel?> ExportAsync(int largeChickenId)
    {
        try
        {
            // 1. Validate that the large chicken exists and is active
            var lc = await _unitOfWork.LargeChickens.GetByIdAsync(largeChickenId);
            if (lc == null || !lc.IsActive.HasValue || !lc.IsActive.Value) return null;

            // 2. Find the chicken barn for this large chicken
            var cb = await _unitOfWork.ChickenBarns.GetByLargeChickenIdAsync(largeChickenId);
            if (cb == null) return null;
            cb.Status = "export"; // Mark as export
            _unitOfWork.ChickenBarns.PrepareUpdate(cb);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            var cbarnId = cb.CbarnId;

            // Now create a fresh context for all modifications to avoid tracking conflicts
            var freshContext = new AutoFeedDBContext();

            try
            {
                // 3. Update the large chicken - mark as inactive
                var largeChicken = await freshContext.LargeChickens.FindAsync(largeChickenId);
                if (largeChicken != null)
                {
                    largeChicken.IsActive = false;
                    freshContext.LargeChickens.Update(largeChicken);
                }

                // 4. Update feeding rules for this large chicken - disable them
                var feedingRules = await freshContext.FeedingRules
                    .Include(r => r.FeedingRuleDetails)
                    .Where(r => r.ChickenLid == largeChickenId)
                    .ToListAsync();
                foreach (var rule in feedingRules)
                {
                    rule.Status = "disabled";
                    freshContext.FeedingRules.Update(rule);

                    // Disable all feeding rule details for this rule
                    foreach (var detail in rule.FeedingRuleDetails)
                    {
                        detail.Status = false;
                        freshContext.FeedingRuleDetails.Update(detail);
                    }
                }
                                

                // 5. Update schedules for this chicken barn - disable them
                var schedules = await freshContext.Schedules
                    .Where(s => s.CbarnId == cbarnId)
                    .ToListAsync();
                foreach (var schedule in schedules)
                {
                    schedule.Status = "disabled";
                    freshContext.Schedules.Update(schedule);
                }

                // 6. Update the chicken barn - mark as exported
                var chickenBarn = await freshContext.ChickenBarns.FindAsync(cbarnId);
                if (chickenBarn != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[EXPORT] Before: CBarnId={chickenBarn.CbarnId}, Status={chickenBarn.Status}");
                    chickenBarn.Status = "export";
                    chickenBarn.ExportDate = DateOnly.FromDateTime(DateTime.Now);
                    freshContext.ChickenBarns.Update(chickenBarn);
                    System.Diagnostics.Debug.WriteLine($"[EXPORT] After marking: CBarnId={chickenBarn.CbarnId}, Status={chickenBarn.Status}");
                }

                // Save all changes
                var rowsAffected = await freshContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"[EXPORT] SaveChangesAsync returned: {rowsAffected} rows affected");

                // Get the final result without tracking - force reload from DB
                var result = await freshContext.ChickenBarns
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CbarnId == cbarnId);

                System.Diagnostics.Debug.WriteLine($"[EXPORT] Final result: CBarnId={result?.CbarnId}, Status={result?.Status}, ExportDate={result?.ExportDate}");
                return result;
            }
            finally
            {
                await freshContext.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExportAsync failed for largeChickenId={largeChickenId}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    public async Task<ChickenBarnModel?> GetByLargeChickenIdAsync(int largeChickenId)
    {
        return await _unitOfWork.ChickenBarns.GetByLargeChickenIdAsync(largeChickenId);
    }


    public async Task<List<ChickenBarnModel>> SearchAsync(int? barnId, int? flockId, int? chickenLid, bool includeInactive = false)
    {
        return await _unitOfWork.ChickenBarns.SearchAsync(barnId, flockId, chickenLid, includeInactive);
    }

    public async Task<List<dynamic>> GetChickenBarnDetailAsync(int? barnId, string? barnType)
    {
        return await _unitOfWork.ChickenBarns.GetChickenBarnDetailAsync(barnId, barnType);
    }
}
