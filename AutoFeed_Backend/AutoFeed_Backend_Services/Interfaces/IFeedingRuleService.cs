using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest;
using AutoFeed_Backend_Services.Models.Responses;

namespace AutoFeed_Backend_Services.Interfaces
{
    public interface IFeedingRuleService
    {
        Task<IEnumerable<object>> GetAllRulesAsync();
        Task<FeedingRuleFullResponse?> GetRuleByIdAsync(int id);
        Task<bool> CreateRuleAsync(FeedingRuleCreateDto dto);
        Task<bool> DisableDetailAsync(int detailId);
        Task<bool> UpdateDetailAsync(int detailId, RuleDetailUpdateDto dto);
        Task<bool> DeleteRuleAsync(int id);
    }
}