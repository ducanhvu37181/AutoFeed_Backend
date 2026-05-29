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

        // Auto-create feeding rule for Flock only (LargeChicken is handled separately in FlockController)
        if (entity.FlockId.HasValue)
        {
            await AutoCreateFeedingRuleAsync(entity);
        }

        return entity.CbarnId;  // Return the actual ID, not the row count
    }

    public async System.Threading.Tasks.Task AutoCreateFeedingRuleForExistingAsync(int cbarnId)
    {
        var chickenBarn = await _unitOfWork.ChickenBarns.GetByIdAsync(cbarnId);
        if (chickenBarn == null) return;

        await AutoCreateFeedingRuleAsync(chickenBarn);
    }

    public async System.Threading.Tasks.Task AutoUpdateFeedingRuleForLargeChickenAsync(int chickenLid)
    {
        try
        {
            // Find the ChickenBarn for this LargeChicken
            var chickenBarns = await _unitOfWork.ChickenBarns.SearchAsync(barnId: null, flockId: null, chickenLid: chickenLid, includeInactive: false);
            var chickenBarn = chickenBarns?.FirstOrDefault();
            if (chickenBarn == null) return;

            // Find the existing FeedingRule for this LargeChicken
            var feedingRules = await _unitOfWork.FeedingRules.GetAllAsync();
            var existingRule = feedingRules.FirstOrDefault(r => r.ChickenLid == chickenLid && r.Status == "active");
            if (existingRule == null) return;

            // Get the updated LargeChicken info
            var largeChicken = await _unitOfWork.LargeChickens.GetByIdAsync(chickenLid);
            if (largeChicken == null) return;

            // Get flock info for chicken type
            var flock = await _unitOfWork.Flocks.GetByIdAsync(largeChicken.FlockId);
            if (flock == null) return;

            // Extract chicken type from flock name
            var chickenType = ExtractChickenType(flock.Name);
            if (string.IsNullOrEmpty(chickenType)) return;

            // Normalize status
            var status = NormalizeStatus(largeChicken.HealthStatus);
            if (string.IsNullOrEmpty(status)) return;

            // Find matching feeding guide (find closest weight match)
            var guides = await _unitOfWork.FeedingGuideLargeChickens.GetAllAsync();
            var guide = guides
                .Where(g => g.ChickenType == chickenType && g.Status == status)
                .OrderBy(g => Math.Abs(g.Weight - largeChicken.Weight))
                .FirstOrDefault();

            if (guide == null) return;

            // Update the feeding rule
            existingRule.Times = guide.Session;
            existingRule.Description = $"Auto-generated from FeedingGuide for {chickenType} {status} (Weight: {largeChicken.Weight}kg)";
            existingRule.Note = guide.Note;

            _unitOfWork.FeedingRules.PrepareUpdate(existingRule);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Delete existing feeding rule details
            var existingDetails = await _unitOfWork.FeedingRuleDetails.GetAllAsync();
            var detailsToDelete = existingDetails.Where(d => d.RuleId == existingRule.RuleId).ToList();
            foreach (var detail in detailsToDelete)
            {
                await _unitOfWork.FeedingRuleDetails.RemoveAsync(detail);
            }
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Create new feeding rule details
            await CreateFeedingRuleDetailsAsync(existingRule.RuleId, guide.Session, guide.FeedPerDay);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AutoUpdateFeedingRuleForLargeChickenAsync failed: {ex.Message}");
        }
    }

    public async System.Threading.Tasks.Task AutoUpdateFeedingRuleForFlockAsync(int flockId)
    {
        try
        {
            // Find the ChickenBarn for this Flock
            var chickenBarns = await _unitOfWork.ChickenBarns.SearchAsync(barnId: null, flockId: flockId, chickenLid: null, includeInactive: false);
            var chickenBarn = chickenBarns?.FirstOrDefault();
            if (chickenBarn == null) return;

            // Find the existing FeedingRule for this Flock
            var feedingRules = await _unitOfWork.FeedingRules.GetAllAsync();
            var existingRule = feedingRules.FirstOrDefault(r => r.FlockId == flockId && r.Status == "active");
            if (existingRule == null) return;

            // Get the updated Flock info
            var flock = await _unitOfWork.Flocks.GetByIdAsync(flockId);
            if (flock == null) return;

            // Extract chicken type from flock name
            var chickenType = ExtractChickenType(flock.Name);
            if (string.IsNullOrEmpty(chickenType)) return;

            // Normalize status
            var status = NormalizeStatus(flock.HealthStatus);
            if (string.IsNullOrEmpty(status)) return;

            // Calculate age in months
            var ageInMonths = CalculateAgeInMonths(flock.DoB);
            if (!ageInMonths.HasValue) return;

            // Find matching feeding guide (find closest age match)
            var guides = await _unitOfWork.FeedingGuideFlocks.GetAllAsync();
            var guide = guides
                .Where(g => g.ChickenType == chickenType && g.Status == status)
                .OrderBy(g => Math.Abs(g.Age - ageInMonths.Value))
                .FirstOrDefault();

            if (guide == null) return;

            // Update the feeding rule
            existingRule.Times = guide.Session;
            existingRule.Description = $"Flock{flockId}_{chickenType}";
            existingRule.Note = guide.Note;

            _unitOfWork.FeedingRules.PrepareUpdate(existingRule);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Delete existing feeding rule details
            var existingDetails = await _unitOfWork.FeedingRuleDetails.GetAllAsync();
            var detailsToDelete = existingDetails.Where(d => d.RuleId == existingRule.RuleId).ToList();
            foreach (var detail in detailsToDelete)
            {
                await _unitOfWork.FeedingRuleDetails.RemoveAsync(detail);
            }
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Create new feeding rule details with flock quantity
            await CreateFeedingRuleDetailsAsync(existingRule.RuleId, guide.Session, guide.FeedPerDay, flock.Quantity);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AutoUpdateFeedingRuleForFlockAsync failed: {ex.Message}");
        }
    }

    public async System.Threading.Tasks.Task SyncAllFeedingRulesFromGuidesAsync()
    {
        try
        {
            // Get all active ChickenBarns
            var chickenBarns = await _unitOfWork.ChickenBarns.GetActiveAsync();
            if (chickenBarns == null || !chickenBarns.Any()) return;

            // Update feeding rules for all flocks
            var flockBarns = chickenBarns.Where(cb => cb.FlockId.HasValue).ToList();
            foreach (var barn in flockBarns)
            {
                await AutoUpdateFeedingRuleForFlockAsync(barn.FlockId.Value);
            }

            // Update feeding rules for all large chickens
            var chickenBarnsList = chickenBarns.Where(cb => cb.ChickenLid.HasValue).ToList();
            foreach (var barn in chickenBarnsList)
            {
                await AutoUpdateFeedingRuleForLargeChickenAsync(barn.ChickenLid.Value);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SyncAllFeedingRulesFromGuidesAsync failed: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task AutoCreateFeedingRuleAsync(ChickenBarnModel chickenBarn)
    {
        try
        {
            // For Flock
            if (chickenBarn.FlockId.HasValue)
            {
                var flock = await _unitOfWork.Flocks.GetByIdAsync(chickenBarn.FlockId.Value);
                if (flock == null) return;

                // Extract chicken type from flock name (e.g., "Flock1_BinhDinh" -> "Binh Dinh")
                var chickenType = ExtractChickenType(flock.Name);
                if (string.IsNullOrEmpty(chickenType)) return;

                // Normalize status
                var status = NormalizeStatus(flock.HealthStatus);
                if (string.IsNullOrEmpty(status)) return;

                // Calculate age in months
                var ageInMonths = CalculateAgeInMonths(flock.DoB);
                if (!ageInMonths.HasValue) return;

                // Find matching feeding guide (find closest age match)
                var guides = await _unitOfWork.FeedingGuideFlocks.GetAllAsync();
                var guide = guides
                    .Where(g => g.ChickenType == chickenType && g.Status == status)
                    .OrderBy(g => Math.Abs(g.Age - ageInMonths.Value))
                    .FirstOrDefault();

                if (guide == null) return;

                // Create feeding rule
                var rule = new FeedingRule
                {
                    FlockId = flock.FlockId,
                    ChickenLid = null,
                    StartDate = chickenBarn.StartDate,
                    EndDate = chickenBarn.StartDate.AddYears(1),
                    Times = guide.Session,
                    Description = $"Flock{flock.FlockId}_{chickenType}",
                    Note = guide.Note,
                    Status = "active"
                };

                _unitOfWork.FeedingRules.PrepareCreate(rule);
                await _unitOfWork.SaveChangesWithTransactionAsync();

                // Create feeding rule details with flock quantity
                await CreateFeedingRuleDetailsAsync(rule.RuleId, guide.Session, guide.FeedPerDay, flock.Quantity);
            }
            // For Large Chicken
            else if (chickenBarn.ChickenLid.HasValue)
            {
                var largeChicken = await _unitOfWork.LargeChickens.GetByIdAsync(chickenBarn.ChickenLid.Value);
                if (largeChicken == null) return;

                // Get flock info for chicken type
                var flock = await _unitOfWork.Flocks.GetByIdAsync(largeChicken.FlockId);
                if (flock == null) return;

                // Extract chicken type from flock name
                var chickenType = ExtractChickenType(flock.Name);
                if (string.IsNullOrEmpty(chickenType)) return;

                // Normalize status
                var status = NormalizeStatus(largeChicken.HealthStatus);
                if (string.IsNullOrEmpty(status)) return;

                // Find matching feeding guide (find closest weight match)
                var guides = await _unitOfWork.FeedingGuideLargeChickens.GetAllAsync();
                var guide = guides
                    .Where(g => g.ChickenType == chickenType && g.Status == status)
                    .OrderBy(g => Math.Abs(g.Weight - largeChicken.Weight))
                    .FirstOrDefault();

                if (guide == null) return;

                // Create feeding rule
                var rule = new FeedingRule
                {
                    ChickenLid = largeChicken.ChickenLid,
                    FlockId = null,
                    StartDate = chickenBarn.StartDate,
                    EndDate = chickenBarn.StartDate.AddYears(1),
                    Times = guide.Session,
                    Description = $"Chicken{largeChicken.ChickenLid}_{chickenType}",
                    Note = guide.Note,
                    Status = "active"
                };

                _unitOfWork.FeedingRules.PrepareCreate(rule);
                await _unitOfWork.SaveChangesWithTransactionAsync();

                // Create feeding rule details
                await CreateFeedingRuleDetailsAsync(rule.RuleId, guide.Session, guide.FeedPerDay);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AutoCreateFeedingRuleAsync failed: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task CreateFeedingRuleDetailsAsync(int ruleId, int session, decimal feedPerDay, int quantity = 1)
    {
        // Get default food (Rice or Corn)
        var foods = await _unitOfWork.Foods.GetAllAsync();
        var defaultFood = foods.FirstOrDefault(f => f.Name == "Rice") ?? foods.FirstOrDefault(f => f.Name == "Corn");
        if (defaultFood == null) return;

        // Calculate total feed per day for the flock (feedPerDay is per chicken)
        var totalFeedPerDay = feedPerDay * quantity;

        // Calculate amount per session
        var amountPerSession = totalFeedPerDay / session;

        // Distribute feeding times (e.g., 8:00, 13:00, 18:00 for 3 sessions)
        var feedTimes = GetFeedTimes(session);

        foreach (var (hour, minute) in feedTimes)
        {
            var detail = new FeedingRuleDetail
            {
                RuleId = ruleId,
                FoodId = defaultFood.FoodId,
                FeedHour = hour,
                FeedMinute = minute,
                Amount = amountPerSession,
                Description = $"Session {feedTimes.IndexOf((hour, minute)) + 1}",
                Status = true
            };

            _unitOfWork.FeedingRuleDetails.PrepareCreate(detail);
        }

        await _unitOfWork.SaveChangesWithTransactionAsync();
    }

    private string ExtractChickenType(string flockName)
    {
        if (string.IsNullOrEmpty(flockName)) return null;

        // Extract chicken type from flock name (e.g., "Flock1_BinhDinh" -> "Binh Dinh")
        var parts = flockName.Split('_');
        if (parts.Length >= 2)
        {
            var typePart = parts[1].ToLower();
            // Map to standardized names
            return typePart switch
            {
                "binhdinh" => "Binh Dinh",
                "caolanh" => "Cao lanh",
                "bentre" => "Ben Tre",
                "doson" => "Do Son",
                "nghitam" => "Nghi Tam",
                _ => null
            };
        }
        return null;
    }

    private string NormalizeStatus(string status)
    {
        if (string.IsNullOrEmpty(status)) return null;

        return status.ToLower() switch
        {
            "healthy" => "Healthy",
            "sick" => "Sick",
            _ => null
        };
    }

    private int? CalculateAgeInMonths(DateOnly dob)
    {
        if (dob == null) return null;

        var today = DateOnly.FromDateTime(DateTime.Now);
        var age = (today.Year - dob.Year) * 12 + (today.Month - dob.Month);
        return age >= 0 ? age : null;
    }

    private List<(int Hour, int Minute)> GetFeedTimes(int session)
    {
        return session switch
        {
            1 => new List<(int, int)> { (8, 0) },
            2 => new List<(int, int)> { (8, 0), (17, 0) },
            3 => new List<(int, int)> { (8, 0), (13, 0), (18, 0) },
            4 => new List<(int, int)> { (7, 0), (11, 0), (15, 0), (19, 0) },
            5 => new List<(int, int)> { (7, 0), (10, 0), (13, 0), (16, 0), (19, 0) },
            _ => new List<(int, int)> { (8, 0), (13, 0), (18, 0) } // Default to 3 sessions
        };
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

            // 2. Find the ACTIVE chicken barn for this large chicken
            var allCbarns = await _unitOfWork.ChickenBarns.GetAllAsync();
            var activeCbarn = allCbarns.FirstOrDefault(cb => cb.ChickenLid == largeChickenId && cb.Status != null && cb.Status.ToLower() == "active");
            if (activeCbarn == null) return null;

            var cbarnId = activeCbarn.CbarnId;

            // 3. Update the large chicken - mark as inactive
            lc.IsActive = false;
            _unitOfWork.LargeChickens.PrepareUpdate(lc);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // 4. Update the ACTIVE chicken barn - mark as exported
            var exportedCbarn = await _unitOfWork.ChickenBarns.ChangeStatusToExportAsync(cbarnId);
            if (exportedCbarn == null) return null;

            // 5. Update feeding rules for this large chicken - disable them
            var allFeedingRules = await _unitOfWork.FeedingRules.GetAllAsync();
            var relevantRules = allFeedingRules.Where(r => r.ChickenLid == largeChickenId).ToList();
            foreach (var rule in relevantRules)
            {
                rule.Status = "disabled";
                _unitOfWork.FeedingRules.PrepareUpdate(rule);
            }
            if (relevantRules.Count > 0)
                await _unitOfWork.SaveChangesWithTransactionAsync();

            // 6. Update schedules for the active chicken barn - disable them
            var allSchedules = await _unitOfWork.Schedules.GetAllAsync();
            var relevantSchedules = allSchedules.Where(s => s.CbarnId == cbarnId).ToList();
            foreach (var schedule in relevantSchedules)
            {
                schedule.Status = "disabled";
                _unitOfWork.Schedules.PrepareUpdate(schedule);
            }
            if (relevantSchedules.Count > 0)
                await _unitOfWork.SaveChangesWithTransactionAsync();

            // Return the exported chicken barn
            return exportedCbarn;
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
