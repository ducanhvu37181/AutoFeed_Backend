using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Cần để dùng ToListAsync cho Migrate
using BCrypt.Net;

// Giải quyết lỗi trùng tên Task giữa System và Model
using Task = System.Threading.Tasks.Task;

namespace AutoFeed_Backend_Services.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    // Giữ nguyên 2 Constructor của bạn Kiên
    public UserService()
    {
        _unitOfWork = new UnitOfWork();
    }

    public UserService(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    // --- CHÈN THÊM: Hàm Migrate để tự động mã hóa dữ liệu cũ ---
    public async Task MigratePasswordsAsync()
    {
        // Phải truy cập qua _unitOfWork.Users hoặc Context tùy theo cấu trúc bạn Kiên
        var users = await _unitOfWork.Users.GetAllAsync();
        bool isChanged = false;

        foreach (var user in users)
        {
            if (!string.IsNullOrEmpty(user.Password) && !user.Password.StartsWith("$2a$"))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                isChanged = true;
                _unitOfWork.Users.PrepareUpdate(user);
            }
        }

        if (isChanged)
        {
            await _unitOfWork.SaveChangesWithTransactionAsync();
        }
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _unitOfWork.Users.GetAllAsync();
    }

    public async System.Threading.Tasks.Task<List<User>> GetActiveAsync()
    {
        return await _unitOfWork.Users.GetActiveAsync();
    }

    public async System.Threading.Tasks.Task<List<User>> GetInactiveAsync()
    {
        return await _unitOfWork.Users.GetInactiveAsync();
    }

    public async System.Threading.Tasks.Task<User?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async System.Threading.Tasks.Task<List<User>> SearchAsync(string? keyword, int? roleId, bool includeInactive)
    {
        return await _unitOfWork.Users.SearchAsync(keyword, roleId, includeInactive);
    }

    public async System.Threading.Tasks.Task<int> CreateAsync(User entity)
    {
        string plainPassword = GenerateRandomPassword();
        var exists = await _unitOfWork.Users.IsEmailOrUsernameExistsAsync(entity.Email, entity.Username);
        if (exists) return -1;

        entity.Status = true;
        // SỬA: Thêm mã hóa BCrypt cho mật khẩu mới
        entity.Password = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        _unitOfWork.Users.PrepareCreate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();

        if (result > 0)
        {
            try
            {
                await _emailService.SendPasswordAsync(
                    toEmail: entity.Email,
                    fullName: entity.FullName ?? entity.Username ?? "User",
                    plainPassword: plainPassword
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Gửi mail thất bại: {ex.Message}");
            }
        }
        return result;
    }

    public async System.Threading.Tasks.Task<bool> UpdateAsync(User entity)
    {
        try
        {
            var duplicate = await _unitOfWork.Users.IsEmailOrUsernameExistsAsync(
                entity.Email, entity.Username, entity.UserId);
            if (duplicate) return false;

            _unitOfWork.Users.PrepareUpdate(entity);
            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }
        catch { return false; }
    }

    public async System.Threading.Tasks.Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(userId);
        if (entity == null || entity.Status != true) return false;

        // SỬA: Dùng BCrypt.Verify để kiểm tra mật khẩu cũ
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, entity.Password)) return false;
        if (BCrypt.Net.BCrypt.Verify(newPassword, entity.Password)) return false;

        entity.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _unitOfWork.Users.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

    public async System.Threading.Tasks.Task<bool> ResetPasswordAsync(string email)
    {
        var entity = await _unitOfWork.Users.GetByEmailAsync(email);
        if (entity == null || entity.Status != true) return false;

        var newPassword = GenerateRandomPassword();
        // SỬA: Hash mật khẩu reset
        entity.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _unitOfWork.Users.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();

        if (result > 0)
        {
            try
            {
                await _emailService.SendPasswordAsync(entity.Email, entity.FullName ?? "User", newPassword);
            }
            catch { }
            return true;
        }
        return false;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = false;
        _unitOfWork.Users.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

    public async System.Threading.Tasks.Task<bool> RestoreAsync(int id)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = true;
        _unitOfWork.Users.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

    public async System.Threading.Tasks.Task<Dictionary<int, string>> GetUserNameMapAsync(IEnumerable<int> userIds)
    {
        var ids = userIds?.ToList() ?? new List<int>();
        var result = new Dictionary<int, string>();
        if (!ids.Any()) return result;
        var all = await _unitOfWork.Users.GetAllAsync();
        return all.Where(u => ids.Contains(u.UserId)).ToDictionary(u => u.UserId, u => u.Username);
    }

    private string GenerateRandomPassword()
    {
        const int passwordLength = 12;
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        var random = new Random();
        return new string(Enumerable.Repeat(validChars, passwordLength)
                                    .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}