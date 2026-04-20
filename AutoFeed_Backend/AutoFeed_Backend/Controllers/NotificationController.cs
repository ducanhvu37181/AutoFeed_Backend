using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET api/Notification
    // Get all notifications for current user
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        // Get userId from JWT token (assumed to be handled in middleware)
        // Temporarily hardcoded userId = 1 for testing, will get from token later
        var userId = 1;

        var notifications = await _notificationService.GetByUserIdAsync(userId);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = notifications,
            Description = "Success"
        });
    }

    // GET api/Notification/unread
    // Get unread notifications for current user
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var userId = 1;

        var notifications = await _notificationService.GetUnreadByUserIdAsync(userId);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = notifications,
            Description = "Success"
        });
    }

    // GET api/Notification/unread-count
    // Get unread notification count for current user
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = 1;

        var count = await _notificationService.GetUnreadCountByUserIdAsync(userId);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = new { unreadCount = count },
            Description = "Success"
        });
    }

    // PUT api/Notification/{id}/read
    // Mark a notification as read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = null,
            Description = "Notification marked as read"
        });
    }

    // PUT api/Notification/read-all
    // Mark all notifications for current user as read
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = 1;

        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new ApiResponse<object>
        {
            Status = true,
            HttpCode = 200,
            Data = null,
            Description = "All notifications marked as read"
        });
    }
}
