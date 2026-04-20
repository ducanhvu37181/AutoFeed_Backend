using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<List<Notification>> GetByUserIdAsync(int userId)
    {
        return await _unitOfWork.Notifications.GetByUserIdAsync(userId);
    }

    public async System.Threading.Tasks.Task<List<Notification>> GetUnreadByUserIdAsync(int userId)
    {
        return await _unitOfWork.Notifications.GetUnreadByUserIdAsync(userId);
    }

    public async System.Threading.Tasks.Task<int> GetUnreadCountByUserIdAsync(int userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountByUserIdAsync(userId);
    }

    public async System.Threading.Tasks.Task MarkAsReadAsync(int notificationId)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
    }

    public async System.Threading.Tasks.Task MarkAllAsReadAsync(int userId)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
    }

    public async System.Threading.Tasks.Task CreateNotificationAsync(int userId, string type, string title, string message, int? relatedId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedId = relatedId,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _unitOfWork.Notifications.PrepareCreate(notification);
        await _unitOfWork.SaveChangesWithTransactionAsync();
    }
}
