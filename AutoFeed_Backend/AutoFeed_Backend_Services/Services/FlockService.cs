using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class FlockService : IFlockService
{
    private readonly IUnitOfWork _unitOfWork;

    public FlockService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<object>> GetFlockDashboardAsync(string? searchTerm, int? barnId, string? status)
    {
        var flocks = await _unitOfWork.Flocks.GetAllWithBarnAsync();
        var query = flocks.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(f => f.FlockId.ToString().Contains(searchTerm) || (f.Name != null && f.Name.Contains(searchTerm)));
        }

        if (barnId.HasValue)
        {
            query = query.Where(f => f.ChickenBarn != null && f.ChickenBarn.BarnId == barnId);
        }

        if (!string.IsNullOrEmpty(status) && status != "All Status")
        {
            query = query.Where(f => f.HealthStatus == status);
        }

        return query.Select(f => new {
            f.FlockId,
            f.Name,
            HatchDate = f.DoB.ToString("yyyy-MM-dd"),
            Age = CalculateAge(f.DoB),
            f.Quantity,
            NurseryBarn = (f.ChickenBarn != null && f.ChickenBarn.Barn != null) ? f.ChickenBarn.Barn.Type : "N/A",
            Status = f.HealthStatus ?? "Healthy",
            f.Note
        }).ToList();
    }

    public async Task<object?> GetFlockDetailAsync(int id)
    {
        var flocks = await _unitOfWork.Flocks.GetAllWithBarnAsync();
        var f = flocks.FirstOrDefault(x => x.FlockId == id);
        if (f == null) return null;

        var schedules = _unitOfWork.Schedules.GetAll()
            .Where(s => s.CbarnId == f.ChickenBarn?.CbarnId)
            .Select(s => new {
                Action = (s.Description != null) ? s.Description : "Daily Action",
                Date = s.StartDate.ToString(),
                // ĐÃ SỬA: s.Status là string nên so sánh với chuỗi "True" hoặc "Completed"
                Status = (s.Status == "True" || s.Status == "Completed") ? "Completed" : "Scheduled"
            }).ToList();

        int startedWith = 50;
        decimal survivalRate = startedWith > 0 ? ((decimal)f.Quantity / startedWith) * 100 : 0;

        return new
        {
            Header = new { f.FlockId, f.Name, Status = f.HealthStatus ?? "Healthy" },
            MainStats = new
            {
                CurrentBirds = f.Quantity,
                StartedWith = startedWith,
                Age = CalculateAge(f.DoB),
                RaisingDate = f.DoB.ToString("yyyy-MM-dd"),
                AvgWeight = f.Weight.ToString() + " kg",
                GrowthStatus = "Normal growth"
            },
            HealthStatistics = new
            {
                Mortality = startedWith - f.Quantity,
                HealthIssues = 2,
                HealthyBirds = f.Quantity - 2,
                SurvivalRate = survivalRate.ToString() + "%"
            },
            PlannedActions = schedules
        };
    }

    public async Task<bool> CreateFlockAsync(FlockChicken flock, int barnId)
    {
        _unitOfWork.Flocks.PrepareCreate(flock);
        bool res = await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
        if (res)
        {
            var cb = new ChickenBarn { FlockId = flock.FlockId, BarnId = barnId, StartDate = flock.DoB, Status = "active" };
            _unitOfWork.ChickenBarns.PrepareCreate(cb);
            await _unitOfWork.SaveChangesWithTransactionAsync();
        }
        return res;
    }

    public async Task<bool> UpdateFlockAsync(FlockChicken flock)
    {
        _unitOfWork.Flocks.PrepareUpdate(flock);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> DeleteFlockAsync(int id)
    {
        var flock = await _unitOfWork.Flocks.GetByIdAsync(id);
        if (flock == null) return false;

        var allCB = await _unitOfWork.ChickenBarns.GetAllAsync();
        var related = allCB.Where(x => x.FlockId == id).ToList();
        foreach (var item in related) _unitOfWork.ChickenBarns.PrepareRemove(item);

        _unitOfWork.Flocks.PrepareRemove(flock);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> AssignIdsToLargeChickensAsync(int id)
    {
        var flock = await _unitOfWork.Flocks.GetByIdAsync(id);
        if (flock == null) return false;
        flock.IsActive = false;
        _unitOfWork.Flocks.PrepareUpdate(flock);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    private string CalculateAge(DateOnly dob)
    {
        var days = (DateTime.Now.Date - dob.ToDateTime(TimeOnly.MinValue)).Days;
        return days >= 7 ? (days / 7).ToString() + " weeks" : days.ToString() + " days";
    }
}