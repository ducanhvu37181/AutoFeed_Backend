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
    // Lấy tất cả notifications của user hiện tại
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        // Lấy userId từ JWT token (giả định đã được xử lý trong middleware)
        // Tạm thời hardcode userId = 1 để test, sau này sẽ lấy từ token
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
    // Lấy các notifications chưa đọc của user hiện tại
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
    // Lấy số lượng notifications chưa đọc của user hiện tại
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
    // Đánh dấu một notification đã đọc
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
    // Đánh dấu tất cả notifications của user hiện tại đã đọc
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
