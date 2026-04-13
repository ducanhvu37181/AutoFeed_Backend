using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest;
using AutoFeed_Backend_Services.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services
{
    public class FeedingRuleService : IFeedingRuleService
    {
        private readonly AutoFeedDBContext _context;
        public FeedingRuleService(AutoFeedDBContext context) => _context = context;

        public async Task<IEnumerable<object>> GetAllRulesAsync()
        {
            return await _context.FeedingRules
                .Select(r => new { r.RuleId, r.Description, r.Times, r.StartDate, r.EndDate })
                .ToListAsync();
        }

        public async Task<FeedingRuleFullResponse?> GetRuleByIdAsync(int id)
        {
            var rule = await _context.FeedingRules
                .Include(r => r.FeedingRuleDetails)
                .ThenInclude(d => d.Food)
                .FirstOrDefaultAsync(r => r.RuleId == id);

            if (rule == null) return null;

            return new FeedingRuleFullResponse
            {
                RuleId = rule.RuleId,
                Description = rule.Description,
                Times = rule.Times,
                StartDate = rule.StartDate,
                EndDate = rule.EndDate,
                Details = rule.FeedingRuleDetails.Select(d => new RuleDetailResponse
                {
                    FeedRuleDetailID = d.FeedRuleDetailId,
                    FoodID = d.FoodId,
                    FoodName = d.Food?.Name ?? "Unknown",
                    FeedHour = d.FeedHour,
                    FeedMinute = d.FeedMinute,
                    Amount = d.Amount,
                    Status = d.Status ?? false,
                }).ToList()
            };
        }

        public async Task<bool> CreateRuleAsync(FeedingRuleCreateDto dto)
{
    if (dto.Times <= 0)
        return false; // Không cho tạo rule nếu times <= 0

    // Chỉ cho phép 1 trong 2 id, không được nhập cả hai hoặc cả hai đều null
    bool hasChicken = dto.ChickenLid != null;
    bool hasFlock = dto.FlockId != null;
    if (hasChicken == hasFlock) // cả hai đều null hoặc cả hai đều có giá trị
        return false;

    // Kiểm tra trùng rule
    if (hasChicken)
    {
        bool exists = await _context.FeedingRules.AnyAsync(r => r.ChickenLid == dto.ChickenLid);
        if (exists) return false;
    }
    else // hasFlock
    {
        bool exists = await _context.FeedingRules.AnyAsync(r => r.FlockId == dto.FlockId);
        if (exists) return false;
    }

    var rule = new FeedingRule
    {
        ChickenLid = hasChicken ? dto.ChickenLid : null,
        FlockId = hasFlock ? dto.FlockId : null,
        Times = dto.Times,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Description = dto.Description,
        Note = dto.Note,
        Status = "active"
    };

    try
    {
        _context.FeedingRules.Add(rule);
        return await _context.SaveChangesAsync() > 0;
    }
    catch (DbUpdateException)
    {
        // swallow DB update exceptions and return false to caller
        return false;
    }
}
        private static int CountActiveDetails(FeedingRule rule) =>
            rule.FeedingRuleDetails.Count(d => d.Status != false);

        public async Task<(bool Success, string Message)> UpdateRuleAsync(int id, FeedingRuleUpdateDto dto)
        {
            var rule = await _context.FeedingRules
                .Include(r => r.FeedingRuleDetails)
                .FirstOrDefaultAsync(r => r.RuleId == id);

            if (rule == null)
                return (false, "Rule not found");

            var activeDetails = CountActiveDetails(rule);
            if (dto.Times < activeDetails)
                return (false, $"Times ({dto.Times}) cannot be less than active feeding details ({activeDetails}).");

            rule.ChickenLid = dto.ChickenLid;
            rule.FlockId = dto.FlockId;
            rule.Times = dto.Times;
            rule.StartDate = dto.StartDate;
            rule.EndDate = dto.EndDate;
            rule.Description = dto.Description;
            rule.Note = dto.Note;

            _context.FeedingRules.Update(rule);
            var saved = await _context.SaveChangesAsync() > 0;
            return saved ? (true, "OK") : (false, "Save failed");
        }

        public async Task<(bool Success, string Message)> AddDetailAsync(RuleDetailCreateDto dto)
        {
            var rule = await _context.FeedingRules
                .Include(r => r.FeedingRuleDetails)
                .FirstOrDefaultAsync(r => r.RuleId == dto.RuleID);

            if (rule == null)
                return (false, "Rule not found");

            var activeCount = CountActiveDetails(rule);
            if (rule.Times <= 0)
                return (false, "Rule Times must be greater than 0 to add feeding details");

            if (activeCount >= rule.Times)
                return (false, $"Cannot add detail: rule already has {activeCount} active meal(s), maximum is {rule.Times} (Times).");

            var detail = new FeedingRuleDetail
            {
                RuleId = dto.RuleID,
                FoodId = dto.FoodID,
                FeedHour = dto.FeedHour,
                FeedMinute = dto.FeedMinute,
                Amount = dto.Amount,
                Description = dto.Description,
                Status = true
            };

            _context.FeedingRuleDetails.Add(detail);
            var saved = await _context.SaveChangesAsync() > 0;
            return saved ? (true, "OK") : (false, "Save failed");
        }

        public async Task<bool> UpdateDetailAsync(int detailId, RuleDetailUpdateDto dto)
        {
            var detail = await _context.FeedingRuleDetails
                .FirstOrDefaultAsync(x => x.FeedRuleDetailId == detailId);

            if (detail == null) return false;

            detail.FoodId = dto.FoodID;
            detail.FeedHour = dto.FeedHour;
            detail.Amount = dto.Amount;
            detail.FeedMinute = dto.FeedMinute;
            detail.Description = dto.Description;

            _context.FeedingRuleDetails.Update(detail);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DisableDetailAsync(int detailId)
        {
            var detail = await _context.FeedingRuleDetails
                .FirstOrDefaultAsync(x => x.FeedRuleDetailId == detailId);

            if (detail == null) return false;

            detail.Status = false;
            _context.FeedingRuleDetails.Update(detail);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _context.FeedingRules.FindAsync(id);
            if (rule == null) return false;
            _context.FeedingRules.Remove(rule);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}