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
    public async Task<IEnumerable<object>> SearchInventoryAsync(string? search)
    {
        var items = await _unitOfWork.Inventories.SearchAsync(search);

        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodName = i.FoodName,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            TotalWeight = i.Quantity * i.WeightPerBag,
            ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd") ?? "N/A",
            Status = i.Status,
            ExpirationStatus = i.ExpiredDate == null
                ? "Unknown"
                : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today)
                    ? "Expired"
                    : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(30))
                        ? "Expiring Soon"
                        : "Good",
        });
    }

    // Lấy tất cả inventory chưa sử dụng
    public async Task<IEnumerable<object>> GetUnusedInventoryAsync()
    {
        var items = await _unitOfWork.Inventories.GetUnusedInventoryAsync();
        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodName = i.FoodName,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            TotalWeight = i.Quantity * i.WeightPerBag,
            ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd") ?? "N/A",
            Status = i.Status,
            ExpirationStatus = i.ExpiredDate == null
                ? "Unknown"
                : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today)
                    ? "Expired"
                    : i.ExpiredDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(30))
                        ? "Expiring Soon"
                        : "Good",
        });
    }

    // Lấy tất cả inventory đã sử dụng
    public async Task<IEnumerable<object>> GetUsedInventoryAsync()
    {
        var items = await _unitOfWork.Inventories.GetUsedInventoryAsync();
        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodName = i.FoodName,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            TotalWeight = i.Quantity * i.WeightPerBag,
            ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd") ?? "N/A",
            Status = i.Status,
            ExpirationStatus = i.ExpiredDate == null
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
            i.InventId,
            i.FoodName,
            i.Quantity,
            i.ExpiredDate,
            DaysLeft = i.ExpiredDate.DayNumber - today.DayNumber
        });
    }

    // Lấy danh sách thức ăn đã hết hạn
    public async Task<IEnumerable<object>> GetExpiredInventoryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var items = await _unitOfWork.Inventories.GetAsync(
            x => x.ExpiredDate < today,
            x => x.ExpiredDate,
            isDescending: false);

        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodName = i.FoodName,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            TotalWeight = i.Quantity * i.WeightPerBag,
            ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd"),
            DaysExpired = today.DayNumber - i.ExpiredDate.DayNumber,
            ImportDate = i.ImportDate.ToString("yyyy-MM-dd")
        });
    }

    // Lấy danh sách thức ăn còn hạn sử dụng
    public async Task<IEnumerable<object>> GetValidInventoryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var items = await _unitOfWork.Inventories.GetAsync(
            x => x.ExpiredDate >= today,
            x => x.ExpiredDate,
            isDescending: true);

        return items.Select(i => new
        {
            InventId = i.InventId,
            FoodName = i.FoodName,
            Quantity = i.Quantity,
            WeightPerBag = i.WeightPerBag,
            TotalWeight = i.Quantity * i.WeightPerBag,
            ExpiredDate = i.ExpiredDate.ToString("yyyy-MM-dd"),
            DaysLeft = i.ExpiredDate.DayNumber - today.DayNumber,
            ImportDate = i.ImportDate.ToString("yyyy-MM-dd")
        });
    }

    // Nhập kho: set status = 'unused' khi thêm mới
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

        // Set ImportDate to today if not provided
        if (item.ImportDate == default)
            item.ImportDate = today;

        // Set status to 'unused' when adding new inventory
        item.Status = "unused";

        var existing = await _unitOfWork.Inventories
            .FirstOrDefaultAsync(x => x.FoodName == item.FoodName
           && x.ExpiredDate == item.ExpiredDate);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
            _unitOfWork.Inventories.PrepareUpdate(existing);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Tạo history record cho update sau khi đã save
            var history = new InventoryHistory
            {
                InventId = existing.InventId,
                FoodName = existing.FoodName,
                OldQuantity = existing.Quantity - item.Quantity,
                NewQuantity = existing.Quantity,
                QuantityChange = item.Quantity,
                WeightPerBag = existing.WeightPerBag,
                ImportDate = existing.ImportDate,
                ExpiredDate = existing.ExpiredDate,
                ChangedAt = DateTime.Now,
                ActionType = "POST"
            };
            _unitOfWork.InventoryHistories.PrepareCreate(history);
            await _unitOfWork.SaveChangesWithTransactionAsync();
        }
        else
        {
            _unitOfWork.Inventories.PrepareCreate(item);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Tạo history record cho insert sau khi đã save để lấy InventId
            var history = new InventoryHistory
            {
                InventId = item.InventId,
                FoodName = item.FoodName,
                OldQuantity = 0,
                NewQuantity = item.Quantity,
                QuantityChange = item.Quantity,
                WeightPerBag = item.WeightPerBag,
                ImportDate = item.ImportDate,
                ExpiredDate = item.ExpiredDate,
                ChangedAt = DateTime.Now,
                ActionType = "POST"
            };
            _unitOfWork.InventoryHistories.PrepareCreate(history);
            await _unitOfWork.SaveChangesWithTransactionAsync();
        }

        return true;
    }

    // Cập nhật số lượng và ngày hết hạn của inventory theo ID
    // Khi quantity thay đổi thì status tự động chuyển thành 'used'
    public async Task<Inventory?> UpdateInventoryAsync(int inventId, int quantity, DateOnly? expiredDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var inventory = await _unitOfWork.Inventories.FirstOrDefaultAsync(x => x.InventId == inventId);
        if (inventory == null)
            return null;

        if (quantity < 0)
            throw new Exception("Quantity cannot be negative");

        if (expiredDate.HasValue && expiredDate.Value <= today)
            throw new Exception("Expired date must be in the future");

        if (expiredDate.HasValue && expiredDate.Value > today.AddYears(2))
            throw new Exception("Expired date too far in future");

        // Lưu quantity cũ để tạo history
        int oldQuantity = inventory.Quantity;

        inventory.Quantity = quantity;

        // Nếu quantity thay đổi thì set status = 'used'
        if (oldQuantity != quantity)
        {
            inventory.Status = "used";

            // Tạo history record cho update
            var history = new InventoryHistory
            {
                InventId = inventory.InventId,
                FoodName = inventory.FoodName,
                OldQuantity = oldQuantity,
                NewQuantity = quantity,
                QuantityChange = quantity - oldQuantity,
                WeightPerBag = inventory.WeightPerBag,
                ImportDate = inventory.ImportDate,
                ExpiredDate = inventory.ExpiredDate,
                ChangedAt = DateTime.Now,
                ActionType = "PUT"
            };
            _unitOfWork.InventoryHistories.PrepareCreate(history);
        }

        if (expiredDate.HasValue)
            inventory.ExpiredDate = expiredDate.Value;

        _unitOfWork.Inventories.PrepareUpdate(inventory);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();

        return result > 0 ? inventory : null;
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
            .GroupBy(x => x.FoodName)
            .Select(g => new
            {
                FoodName = g.Key,
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
            .GroupBy(x => x.FoodName)
            .Select(g =>
            {
                var nearest = g.OrderBy(x => x.ExpiredDate).First();

                return new
                {
                    FoodName = g.Key,
                    ExpiredDate = nearest.ExpiredDate,
                    Quantity = nearest.Quantity
                };
            });
    }

    // Lấy lịch sử inventory theo inventory ID
    public async Task<IEnumerable<object>> GetInventoryHistoryAsync(int inventId)
    {
        var items = await _unitOfWork.InventoryHistories.GetHistoryByInventoryIdAsync(inventId);
        return items.Select(h => new
        {
            HistoryId = h.HistoryId,
            InventId = h.InventId,
            FoodName = h.FoodName,
            OldQuantity = h.OldQuantity,
            NewQuantity = h.NewQuantity,
            QuantityChange = h.QuantityChange,
            WeightPerBag = h.WeightPerBag,
            TotalWeight = h.NewQuantity * h.WeightPerBag,
            ImportDate = h.ImportDate.ToString("yyyy-MM-dd"),
            ExpiredDate = h.ExpiredDate.ToString("yyyy-MM-dd"),
            ChangedAt = h.ChangedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            ChangedBy = h.ChangedBy,
            ActionType = h.ActionType
        });
    }

    // Lấy tất cả lịch sử inventory
    public async Task<IEnumerable<object>> GetAllInventoryHistoryAsync()
    {
        var items = await _unitOfWork.InventoryHistories.GetAllHistoryAsync();
        return items.Select(h => new
        {
            HistoryId = h.HistoryId,
            InventId = h.InventId,
            FoodName = h.FoodName,
            OldQuantity = h.OldQuantity,
            NewQuantity = h.NewQuantity,
            QuantityChange = h.QuantityChange,
            WeightPerBag = h.WeightPerBag,
            TotalWeight = h.NewQuantity * h.WeightPerBag,
            ImportDate = h.ImportDate.ToString("yyyy-MM-dd"),
            ExpiredDate = h.ExpiredDate.ToString("yyyy-MM-dd"),
            ChangedAt = h.ChangedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            ChangedBy = h.ChangedBy,
            ActionType = h.ActionType
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