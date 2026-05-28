using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class FeedingRuleDetailRepository : GenericRepository<FeedingRuleDetail>
{
    public FeedingRuleDetailRepository() : base() { }

    public FeedingRuleDetailRepository(AutoFeedDBContext context) : base(context) { }

    public new async Task<List<FeedingRuleDetail>> GetAllAsync()
    {
        return await _context.Set<FeedingRuleDetail>()
            .Include(d => d.Food)
            .ToListAsync();
    }

    public new async Task<FeedingRuleDetail?> GetByIdAsync(int id)
    {
        return await _context.Set<FeedingRuleDetail>()
            .Include(d => d.Food)
            .FirstOrDefaultAsync(x => x.FeedRuleDetailId == id);
    }

    public async Task<List<FeedingRuleDetail>> GetByRuleIdAsync(int ruleId)
    {
        return await _context.Set<FeedingRuleDetail>()
            .Include(d => d.Food)
            .Where(x => x.RuleId == ruleId)
            .ToListAsync();
    }
}
