using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services
{
    public class FeedingGuideFlockService : IFeedingGuideFlockService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeedingGuideFlockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<FeedingGuideFlock>> GetAllAsync()
        {
            return await _unitOfWork.FeedingGuideFlocks.GetAllAsync();
        }

        public async Task<FeedingGuideFlock?> GetByIdAsync(int id)
        {
            return await _unitOfWork.FeedingGuideFlocks.GetByIdAsync(id);
        }

        public async Task<List<FeedingGuideFlock>> GetByChickenTypeAsync(string chickenType)
        {
            return await _unitOfWork.FeedingGuideFlocks.SearchAsync(chickenType: chickenType, status: null, age: null);
        }

        public async Task<FeedingGuideFlock?> CreateAsync(FeedingGuideFlock entity)
        {
            await _unitOfWork.FeedingGuideFlocks.CreateAsync(entity);
            return entity;
        }

        public async Task<bool> UpdateAsync(FeedingGuideFlock entity)
        {
            await _unitOfWork.FeedingGuideFlocks.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.FeedingGuideFlocks.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.FeedingGuideFlocks.RemoveAsync(entity);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }
    }
}
