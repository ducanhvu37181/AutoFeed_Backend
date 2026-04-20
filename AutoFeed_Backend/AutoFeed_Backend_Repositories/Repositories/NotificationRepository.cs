using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class NotificationRepository : GenericRepository<Notification>
{
    public NotificationRepository() : base() { }
    public NotificationRepository(AutoFeedDBContext context) : base(context) { }

    public async System.Threading.Tasks.Task<List<Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<List<Notification>> GetUnreadByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<int> GetUnreadCountByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async System.Threading.Tasks.Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async System.Threading.Tasks.Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            _context.Notifications.Update(notification);
        }

        await _context.SaveChangesAsync();
    }
}
