using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Search food trong inventory — trả về object gọn cho frontend
    public async Task<IEnumerable<object>> SearchInventoryAsync(string? search, string? type)
    {
        var items = await _unitOfWork.Inventories.SearchAsync(search, type);

        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodId = i.FoodId,
            FoodName = i.Food?.Name,
            FoodType = i.Food?.Type,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            TotalWeight = i.Quantity * i.WeightPerBag,
            ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd") ?? "N/A",
            Status = i.ExpiredDate == null
    ? "Unknown"
    : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today)
        ? "Expired"
        : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            ? "Expiring Soon"
            : "Good",
        });
    }

    // Lấy danh sách sắp hết hạn
    public async Task<IEnumerable<object>> GetExpiringSoonAsync(int days = 30)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var items = await _unitOfWork.Inventories.GetAsync(
            x => x.Quantity > 0,
            x => x.ExpiredDate <= today.AddDays(days),
            isDescending: false);

        return items.Select(i => new
        {
            i.FoodId,
            FoodName = i.Food?.Name,
            i.Quantity,
            i.ExpiredDate,
            DaysLeft = i.ExpiredDate.DayNumber - today.DayNumber
        });
    }

    // Nhập kho: tăng quantity sau khi nhập kho thành công 
    public async Task<bool> AddInventoryAsync(Inventory item)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (item.ExpiredDate <= today)
            throw new Exception("Cannot add expired food");

        if (item.ExpiredDate > today.AddYears(2))
            throw new Exception("Expired date too far in future");

        if (item.WeightPerBag <= 0)
            throw new Exception("Invalid weight");

        if (item.Quantity <= 0)
            throw new Exception("Invalid quantity");

        var existing = await _unitOfWork.Inventories
            .FirstOrDefaultAsync(x => x.FoodId == item.FoodId
           && x.ExpiredDate == item.ExpiredDate);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
            _unitOfWork.Inventories.PrepareUpdate(existing);
        }
        else
        {
            _unitOfWork.Inventories.PrepareCreate(item);
        }

        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    //Xuất kho: farmer lấy food từ inventory để cho vào máy cho ăn tự động
    //public async Task<bool> ConsumeInventoryAsync(int foodId, int quantity)
    //{
    //    var inventories = await _unitOfWork.Inventories.GetAsync(
    //        x => x.FoodId == foodId && x.Quantity > 0,
    //        x => x.ExpiredDate,
    //        isDescending: false
    //    );

    //    decimal remaining = quantity;

    //    foreach (var item in inventories)
    //    {
    //        if (remaining <= 0) break;

    //        if (item.Quantity >= remaining)
    //        {
    //            item.Quantity -= remaining;
    //            remaining = 0;
    //        }
    //        else
    //        {
    //            remaining -= item.Quantity;
    //            item.Quantity = 0;
    //        }

    //        _unitOfWork.Inventories.PrepareUpdate(item);
    //    }

    //    if (remaining > 0)
    //        throw new Exception("Not enough inventory");

    //    return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    //}

    // Farmer gửi request xin nhập thêm hàng — tạo Request với type = "Inventory"
    //public async Task<bool> RequestNewItemAsync(int userId, string foodName, string description)
    //{
    //    var request = new Request
    //    {
    //        UserId = userId,
    //        Type = "Inventory",
    //        Description = $"Yêu cầu nhập thêm: {foodName}. {description}".Trim(),
    //        Status = "pending",
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    _unitOfWork.Requests.PrepareCreate(request);
    //    return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    //}

    //Lấy tổng kho, grouped theo food
    public async Task<IEnumerable<object>> GetInventorySummaryAsync()
    {
        var items = await _unitOfWork.Inventories.GetAllAsync();

        return items
            .GroupBy(x => x.FoodId)
            .Select(g => new
            {
                FoodId = g.Key,
                FoodName = g.First().Food?.Name,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalWeight = g.Sum(x => x.Quantity * x.WeightPerBag),

                // gần hết hạn nhất
                NearestExpiredDate = g.Min(x => x.ExpiredDate)
            });
    }

    //Lấy patch gần hết hạn nhất
    public async Task<IEnumerable<object>> GetNearestExpiredAsync()
    {
        var items = await _unitOfWork.Inventories.GetAllAsync();

        return items
            .GroupBy(x => x.FoodId)
            .Select(g =>
            {
                var nearest = g.OrderBy(x => x.ExpiredDate).First();

                return new
                {
                    FoodId = g.Key,
                    FoodName = nearest.Food?.Name,
                    ExpiredDate = nearest.ExpiredDate,
                    Quantity = nearest.Quantity
                };
            });
    }

    // Tạo feeding session, để khi farmer lấy bao thức ăn dùng còn dư thì không bị mất dữ liệu
    //public async Task<int> CreateFeedingSessionAsync(int foodId, decimal quantity)
    //{
    //    var inventories = await _unitOfWork.Inventories
    //        .GetAsync(x => x.FoodId == foodId && x.Quantity > 0,
    //                  x => x.ExpiredDate,
    //                  isDescending: false);

    //    decimal remaining = quantity;

    //    var session = new FeedingSession
    //    {
    //        FoodId = foodId,
    //        PlannedQuantity = quantity,
    //        Status = "Pending",
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    _unitOfWork.FeedingSessions.PrepareCreate(session);
    //    await _unitOfWork.SaveChangesWithTransactionAsync(); 

    //    foreach (var item in inventories)
    //    {
    //        if (remaining <= 0) break;

    //        decimal taken;

    //        if (item.Quantity >= remaining)
    //        {
    //            taken = remaining;
    //            item.Quantity -= remaining;
    //            remaining = 0;
    //        }
    //        else
    //        {
    //            taken = item.Quantity;
    //            remaining -= item.Quantity;
    //            item.Quantity = 0;
    //        }

    //        // Lưu detail
    //        _unitOfWork.FeedingSessionDetails.PrepareCreate(new FeedingSessionDetail
    //        {
    //            SessionId = session.SessionId,
    //            InventId = item.InventId,
    //            Quantity = taken
    //        });

    //        _unitOfWork.Inventories.PrepareUpdate(item);
    //    }

    //    if (remaining > 0)
    //        throw new Exception("Not enough inventory");

    //    await _unitOfWork.SaveChangesWithTransactionAsync();

    //    return session.SessionId;
    //}
    
    //// Farmer confirm lại số lượng dùng
    //public async Task<bool> CompleteFeedingSessionAsync(int sessionId, decimal actualQuantity)
    //{
    //    var session = await _unitOfWork.FeedingSessions.GetByIdAsync(sessionId);

    //    if (session == null)
    //        throw new Exception("Session not found");

    //    var details = await _unitOfWork.FeedingSessionDetails
    //        .GetAsync(x => x.SessionId == sessionId,
    //        x => x.DetailId,
    //        isDescending: true);

    //    decimal planned = session.PlannedQuantity;
    //    decimal diff = planned - actualQuantity;

    //    if (diff > 0)
    //    {
    //        // trả lại kho
    //        foreach (var d in details.OrderByDescending(x => x.SessionId))
    //        {
    //            if (diff <= 0) break;

    //            var inventory = await _unitOfWork.Inventories
    //                .GetByIdAsync(d.InventId);

    //            decimal giveBack = Math.Min(diff, d.Quantity);

    //            inventory.Quantity += giveBack;
    //            diff -= giveBack;

    //            _unitOfWork.Inventories.PrepareUpdate(inventory);
    //        }
    //    }

    //    session.ActualQuantity = actualQuantity;
    //    session.Status = "completed";

    //    _unitOfWork.FeedingSessions.PrepareUpdate(session);

    //    return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    //}
}