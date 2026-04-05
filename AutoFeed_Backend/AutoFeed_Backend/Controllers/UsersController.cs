using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend.Models.Requests;
using AutoFeed_Backend.Models.Requests.User;
using AutoFeed_Backend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    // POST /api/user/{id}/avatar
    // Content-Type: multipart/form-data
    [HttpPost("{id:int}/avatar")]
    public async Task<IActionResult> UploadAvatar(int id, IFormFile file, [FromServices] IConfiguration config)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "No file uploaded" });

        var user = await _service.GetByIdAsync(id);
        if (user == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "User not found" });

        var bucket = config.GetValue<string>("Firebase:Bucket");
        var saPath = config.GetValue<string>("Firebase:ServiceAccountPath");
        if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(saPath))
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Firebase not configured" });

        try
        {
            // allow common image types only
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid image type" });

            var url = await AutoFeed_Backend.Helpers.FirebaseStorageHelper.UploadFileAndGetSignedUrlAsync(file, bucket, saPath, TimeSpan.FromDays(30));
            user.AvatarUrl = url;
            var ok = await _service.UpdateAsync(user);
            if (!ok)
                return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Save avatar failed" });

            return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = url, Description = "Avatar uploaded" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        var dto = items.Select(u => new AutoFeed_Backend.Models.Responses.UserResponse {
            UserId = u.UserId,
            RoleId = u.RoleId,
            Email = u.Email,
            FullName = u.FullName,
            Phone = u.Phone,
            Username = u.Username,
            AvatarUrl = u.AvatarUrl,
            Status = u.Status
        }).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var items = await _service.GetActiveAsync();
        var dto = items.Select(u => new AutoFeed_Backend.Models.Responses.UserResponse {
            UserId = u.UserId,
            RoleId = u.RoleId,
            Email = u.Email,
            FullName = u.FullName,
            Phone = u.Phone,
            Username = u.Username,
            AvatarUrl = u.AvatarUrl,
            Status = u.Status
        }).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive()
    {
        var items = await _service.GetInactiveAsync();
        var dto = items.Select(u => new AutoFeed_Backend.Models.Responses.UserResponse {
            UserId = u.UserId,
            RoleId = u.RoleId,
            Email = u.Email,
            FullName = u.FullName,
            Phone = u.Phone,
            Username = u.Username,
            AvatarUrl = u.AvatarUrl,
            Status = u.Status
        }).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });

        // map to response DTO and hide password
        var dto = new AutoFeed_Backend.Models.Responses.UserResponse
        {
            UserId = item.UserId,
            RoleId = item.RoleId,
            Email = item.Email,
            FullName = item.FullName,
            Phone = item.Phone,
            Username = item.Username,
            AvatarUrl = item.AvatarUrl,
            Status = item.Status
        };

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    // GET api/user/search?keyword=john&roleId=2&includeInactive=false
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] int? roleId,
        [FromQuery] bool includeInactive = false)
    {
        var items = await _service.SearchAsync(keyword, roleId, includeInactive);
        var dto = items.Select(u => new AutoFeed_Backend.Models.Responses.UserResponse {
            UserId = u.UserId,
            RoleId = u.RoleId,
            Email = u.Email,
            FullName = u.FullName,
            Phone = u.Phone,
            Username = u.Username,
            AvatarUrl = u.AvatarUrl,
            Status = u.Status
        }).ToList();
        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = dto, Description = "Success" });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email))
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var entity = new User
        {
            RoleId = model.RoleId ?? 0,
            Email = model.Email,
            FullName = model.FullName,
            Phone = model.Phone,
            Username = model.Username
        };

        //var result = await _service.CreateAsync(entity);
        var result = await _service.CreateAsync(entity);
        if (result == -1)
            return Conflict(new ApiResponse<object> { Status = false, HttpCode = 409, Data = null, Description = "Email or Username already exists" });

        if (result <= 0)
            return StatusCode(500, new ApiResponse<object> { Status = false, HttpCode = 500, Data = null, Description = "Create failed" });

        entity.Password = null;
        return CreatedAtAction(nameof(Get), new { id = entity.UserId },
            new ApiResponse<object> { Status = true, HttpCode = 201, Data = entity, Description = "Created" });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest model)
    {
        if (model == null)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found" });

        existing.RoleId = model.RoleId ?? existing.RoleId;
        existing.Email = model.Email;
        existing.FullName = model.FullName;
        existing.Phone = model.Phone;
        existing.Username = model.Username;

        var ok = await _service.UpdateAsync(existing);
        if (!ok)
            return Conflict(new ApiResponse<object> { Status = false, HttpCode = 409, Data = null, Description = "Update failed or duplicate email/username" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "Update success" });
    }

    [HttpPatch("{id:int}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.OldPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Invalid request" });

        if (model.OldPassword == model.NewPassword)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "New password must be different from old password" });

        var ok = await _service.ChangePasswordAsync(id, model.OldPassword, model.NewPassword);
        if (!ok)
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "User not found or wrong old password" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "Password changed successfully" });
    }

    // Soft delete
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found or Delete failed" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "Delete success" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email))
            return BadRequest(new ApiResponse<object> { Status = false, HttpCode = 400, Data = null, Description = "Email is required" });

        var ok = await _service.ResetPasswordAsync(model.Email);
        if (!ok)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "User not found or inactive" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "New password sent to email" });
    }

    // Restore
    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var ok = await _service.RestoreAsync(id);
        if (!ok)
            return NotFound(new ApiResponse<object> { Status = false, HttpCode = 404, Data = null, Description = "Not Found or Restore failed" });

        return Ok(new ApiResponse<object> { Status = true, HttpCode = 200, Data = null, Description = "Restore success" });
    }
}