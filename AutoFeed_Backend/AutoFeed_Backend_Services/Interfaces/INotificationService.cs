using AutoFeed_Backend_DAO.Models;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface INotificationService
{
    System.Threading.Tasks.Task<List<Notification>> GetByUserIdAsync(int userId);
    System.Threading.Tasks.Task<List<Notification>> GetUnreadByUserIdAsync(int userId);
    System.Threading.Tasks.Task<int> GetUnreadCountByUserIdAsync(int userId);
    System.Threading.Tasks.Task MarkAsReadAsync(int notificationId);
    System.Threading.Tasks.Task MarkAllAsReadAsync(int userId);
    System.Threading.Tasks.Task CreateNotificationAsync(int userId, string type, string title, string message, int? relatedId = null);
}
