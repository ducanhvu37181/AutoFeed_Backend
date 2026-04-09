using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services
{
    public class FoodService : IFoodService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Food>> GetAllFoodsAsync()
        {
            return await _unitOfWork.Foods.GetAllAsync();
        }

        public async Task<Food?> GetFoodByIdAsync(int id)
        {
            return await _unitOfWork.Foods.GetByIdAsync(id);
        }

        public async Task<bool> AddFoodItemAsync(Food item)
        {
                     await _unitOfWork.Foods.CreateAsync(item);

            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }

        public async Task<bool> UpdateFoodAsync(Food item)
        {
            var existingFood = await _unitOfWork.Foods.GetByIdAsync(item.FoodId);
            if (existingFood == null) return false;

            await _unitOfWork.Foods.UpdateAsync(item);
            await _unitOfWork.SaveChangesWithTransactionAsync();
            return true;
        }

        public async Task<bool> DeleteFoodAsync(int id)
        {
            var food = await _unitOfWork.Foods.GetByIdAsync(id);
            if (food == null) return false;

            _unitOfWork.Foods.Remove(food);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return true;
        }
    }
}