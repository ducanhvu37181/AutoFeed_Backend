using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services
{
    public class FeedingGuideLargeChickenService : IFeedingGuideLargeChickenService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeedingGuideLargeChickenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<FeedingGuideLargeChicken>> GetAllAsync()
        {
            return await _unitOfWork.FeedingGuideLargeChickens.GetAllAsync();
        }

        public async Task<FeedingGuideLargeChicken?> GetByIdAsync(int id)
        {
            return await _unitOfWork.FeedingGuideLargeChickens.GetByIdAsync(id);
        }

        public async Task<List<FeedingGuideLargeChicken>> GetByChickenTypeAsync(string chickenType)
        {
            return await _unitOfWork.FeedingGuideLargeChickens.SearchAsync(chickenType: chickenType, status: null, weight: null);
        }

        public async Task<FeedingGuideLargeChicken?> CreateAsync(FeedingGuideLargeChicken entity)
        {
            await _unitOfWork.FeedingGuideLargeChickens.CreateAsync(entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(FeedingGuideLargeChicken entity)
        {
            await _unitOfWork.FeedingGuideLargeChickens.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.FeedingGuideLargeChickens.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.FeedingGuideLargeChickens.RemoveAsync(entity);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }
    }
}
