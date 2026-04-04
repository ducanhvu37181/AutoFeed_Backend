using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;

namespace AutoFeed_Backend_Services.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public UserService()
    {
        _unitOfWork = new UnitOfWork();
    }

    public UserService(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _unitOfWork.Users.GetAllAsync();
    }

    public async Task<List<User>> GetActiveAsync()
    {
        return await _unitOfWork.Users.GetActiveAsync();
    }

    public async Task<List<User>> GetInactiveAsync()
    {
        return await _unitOfWork.Users.GetInactiveAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async Task<List<User>> SearchAsync(string? keyword, int? roleId, bool includeInactive)
    {
        return await _unitOfWork.Users.SearchAsync(keyword, roleId, includeInactive);
    }

    public async Task<int> CreateAsync(User entity, string plainPassword)
    {
        // Check duplicate email/username
        var exists = await _unitOfWork.Users.IsEmailOrUsernameExistsAsync(entity.Email, entity.Username);
        if (exists) return -1;

        entity.Status = true;
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
                // Gửi mail thất bại không ảnh hưởng việc tạo user
                Console.WriteLine($"[EmailService] Gửi mail thất bại: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<bool> UpdateAsync(User entity)
    {
        try
        {
            // Check duplicate email/username excluding self
            var duplicate = await _unitOfWork.Users.IsEmailOrUsernameExistsAsync(
                entity.Email, entity.Username, entity.UserId);
            if (duplicate) return false;

            _unitOfWork.Users.PrepareUpdate(entity);
            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    //public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    //{
    //    var entity = await _unitOfWork.Users.GetByIdAsync(userId);
    //    if (entity == null || entity.Status != true) return false;

    //    if (!BCrypt.Net.BCrypt.Verify(oldPassword, entity.Password)) return false;

    //    entity.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
    //    _unitOfWork.Users.PrepareUpdate(entity);
    //    var result = await _unitOfWork.SaveChangesWithTransactionAsync();
    //    return result > 0;
    //}

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = false;
        _unitOfWork.Users.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = true;
        _unitOfWork.Users.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

    public async Task<Dictionary<int, string>> GetUserNameMapAsync(IEnumerable<int> userIds)
    {
        var ids = userIds?.ToList() ?? new List<int>();
        var result = new Dictionary<int, string>();
        if (!ids.Any()) return result;
        var all = await _unitOfWork.Users.GetAllAsync();
        return all.Where(u => ids.Contains(u.UserId)).ToDictionary(u => u.UserId, u => u.Username);
    }

    public Task<int> CreateAsync(User entity)
    {
        throw new NotImplementedException();
    }
}