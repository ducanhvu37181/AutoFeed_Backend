using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class FeedingRuleRepository : GenericRepository<FeedingRule>
{
    public FeedingRuleRepository() : base() { }
    public FeedingRuleRepository(AutoFeedDBContext context) : base(context) { }

    public async Task<List<FeedingRule>> GetByChickenIdAsync(int chickenLid)
    {
        return await _context.FeedingRules
            .Include(r => r.FeedingRuleDetails)
            .Where(r => r.ChickenLid == chickenLid)
            .ToListAsync();
    }
}
