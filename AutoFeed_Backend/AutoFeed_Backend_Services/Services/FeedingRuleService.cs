using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoFeed_Backend_Services.Services;

public class FeedingRuleService : IFeedingRuleService
{
    private readonly AutoFeedDBContext _context;

    public FeedingRuleService(AutoFeedDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FeedingRule>> GetAllRulesAsync()
    {
        return await _context.FeedingRules.ToListAsync();
    }

    public async Task<FeedingRule> GetRuleByIdAsync(int id)
    {
        return await _context.FeedingRules.FindAsync(id);
    }

    public async Task<FeedingRule> CreateRuleAsync(FeedingRuleCreateDto dto)
    {
        var rule = new FeedingRule
        {
            ChickenLid = dto.ChickenLid,
            FlockId = dto.FlockId,
            Times = dto.Times,
            Description = dto.Description,
            Note = dto.Category + ": " + dto.Note
        };
        _context.FeedingRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<bool> UpdateRuleAsync(int id, FeedingRule rule)
    {
        if (id != rule.RuleId) return false;
        _context.Entry(rule).State = EntityState.Modified;
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