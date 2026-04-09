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
            // Sử dụng CreateAsync theo gợi ý từ Repository của bạn
            await _unitOfWork.Foods.CreateAsync(item);

            // Ép lưu xuống DB và kiểm tra kết quả
            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }

        public async Task<bool> UpdateFoodAsync(Food item)
        {
            await _unitOfWork.Foods.UpdateAsync(item);
            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }

        public async Task<bool> DeleteFoodAsync(int id)
        {
            var food = await _unitOfWork.Foods.GetByIdAsync(id);
            if (food == null) return false;

            // Kiểm tra xem Repo dùng Remove hay Delete, thường là Remove
            _unitOfWork.Foods.Remove(food);
            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }
    }
}