using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces
{
    public interface IFeedingGuideLargeChickenService
    {
        Task<List<FeedingGuideLargeChicken>> GetAllAsync();
        Task<FeedingGuideLargeChicken?> GetByIdAsync(int id);
        Task<List<FeedingGuideLargeChicken>> GetByChickenTypeAsync(string chickenType);
        Task<FeedingGuideLargeChicken?> CreateAsync(FeedingGuideLargeChicken entity);
        Task<bool> UpdateAsync(FeedingGuideLargeChicken entity);
        Task<bool> DeleteAsync(int id);
    }
}
