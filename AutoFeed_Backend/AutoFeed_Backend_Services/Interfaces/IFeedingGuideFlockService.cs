using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces
{
    public interface IFeedingGuideFlockService
    {
        Task<List<FeedingGuideFlock>> GetAllAsync();
        Task<FeedingGuideFlock?> GetByIdAsync(int id);
        Task<List<FeedingGuideFlock>> GetByChickenTypeAsync(string chickenType);
        Task<FeedingGuideFlock?> CreateAsync(FeedingGuideFlock entity);
        Task<bool> UpdateAsync(FeedingGuideFlock entity);
        Task<bool> DeleteAsync(int id);
    }
}
