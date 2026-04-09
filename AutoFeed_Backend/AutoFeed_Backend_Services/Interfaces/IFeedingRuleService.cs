using AutoFeed_Backend_DAO.Models;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IFeedingRuleService
{
    Task<IEnumerable<FeedingRule>> GetAllRulesAsync();
    Task<FeedingRule> GetRuleByIdAsync(int id);
    Task<FeedingRule> CreateRuleAsync(FeedingRuleCreateDto dto);
    Task<bool> UpdateRuleAsync(int id, FeedingRule rule);
    Task<bool> DeleteRuleAsync(int id);
}