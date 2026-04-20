using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class BarnService : IBarnService
{
    private readonly IUnitOfWork _unitOfWork;

    public BarnService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Barn>> GetAllAsync() => await _unitOfWork.Barns.GetAllAsync();

    public async Task<Barn?> GetByIdAsync(int id) => await _unitOfWork.Barns.GetByIdAsync(id);

    public async Task<List<Barn>> GetAvailableAsync() => await _unitOfWork.Barns.GetAvailableAsync();

    public async Task<List<Barn>> GetAvailableByTypeAsync(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return new List<Barn>();

        var allAvailable = await _unitOfWork.Barns.GetAvailableAsync();
        return allAvailable.Where(b => b.Type?.Equals(type, System.StringComparison.OrdinalIgnoreCase) ?? false).ToList();
    }

    public async Task<bool> CreateBarnAsync(Barn barn)
    {
        _unitOfWork.Barns.PrepareCreate(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> UpdateBarnAsync(Barn barn)
    {
        _unitOfWork.Barns.PrepareUpdate(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> DeleteBarnAsync(int id)
    {
        var barn = await _unitOfWork.Barns.GetByIdAsync(id);
        if (barn == null) return false;

        _unitOfWork.Barns.PrepareRemove(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> UpdateBarnMetricsAsync(int barnId, decimal temperature, decimal humidity)
    {
        var barn = await _unitOfWork.Barns.GetByIdAsync(barnId);
        if (barn == null) return false;

        barn.Temperature = temperature;
        barn.Humidity = humidity;

        _unitOfWork.Barns.PrepareUpdate(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> UpdateFoodAmountAsync(int barnId, decimal foodAmount)
    {
        var barn = await _unitOfWork.Barns.GetByIdAsync(barnId);
        if (barn == null) return false;

        barn.FoodAmount = foodAmount;
        _unitOfWork.Barns.PrepareUpdate(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> UpdateWaterAmountAsync(int barnId, int waterAmount)
    {
        var barn = await _unitOfWork.Barns.GetByIdAsync(barnId);
        if (barn == null) return false;

        barn.WaterAmount = waterAmount;
        _unitOfWork.Barns.PrepareUpdate(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<string> GetBarnStatusAsync(int barnId)
    {
        // Check if barn has any active ChickenBarn record (status != "export")
        var hasActiveChickenBarn = await _unitOfWork.ChickenBarns.IsActiveAsync(barnId);
        return hasActiveChickenBarn ? "used" : "empty";
    }

    public async Task<decimal> GetFoodWeekAsync(int barnId)
    {
        var today = DateTime.Now.Date;
        var diff = (7 + ((int)today.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        var mondayThisWeek = today.AddDays(-diff);
        var mondayLastWeek = mondayThisWeek.AddDays(-7);
        var sundayLastWeek = mondayLastWeek.AddDays(6);
        return await _unitOfWork.DataIoTs.GetTotalFoodByDateRangeAsync(barnId, mondayLastWeek, sundayLastWeek);
    }

    public async Task<decimal> GetFoodMonthAsync(int barnId)
    {
        var today = DateTime.Now.Date;
        var firstDay = new DateTime(today.Year, today.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        return await _unitOfWork.DataIoTs.GetTotalFoodByDateRangeAsync(barnId, firstDay, lastDay);
    }
}