using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;

namespace AutoFeed_Backend_Services.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    private static readonly string[] AllowedStatuses = { "pending", "reviewed", "rejected" };

    public ReportService()
    {
        _unitOfWork = new UnitOfWork();
    }

    public ReportService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

  

    public async Task<int> CreateReportAsync(Report report)
    {
        report.Status = "pending";
        report.CreateDate = DateTime.Now;
        _unitOfWork.Reports.PrepareCreate(report);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();

        // Tạo notification cho managers khi có report mới
        if (result > 0 && _notificationService != null)
        {
            // Load user để lấy FullName
            var user = await _unitOfWork.Users.GetByIdAsync(report.UserId);
            var managers = await _unitOfWork.Users.GetAllByRoleDescriptionContainsAsync("manager");
            foreach (var manager in managers)
            {
                await _notificationService.CreateNotificationAsync(
                    manager.UserId,
                    "Report",
                    "New Report",
                    $"You have a notification from {user?.FullName ?? "Unknown"}",
                    report.ReportId
                );
            }
        }

        return result;
    }

    

    public async Task<Report?> GetReportByIdAsync(int id)
        => await _unitOfWork.Reports.GetByIdWithUserAsync(id);

    public async Task<List<Report>> GetAllReportsAsync()
        => await _unitOfWork.Reports.GetAllWithUserAsync();

    public async Task<List<Report>> GetFarmerReportsAsync()
        => await _unitOfWork.Reports.GetByUserRoleAsync("farmer");

    public async Task<List<Report>> GetTechFarmerReportsAsync()
        => await _unitOfWork.Reports.GetByUserRoleAsync("techfarmer");

    public async Task<List<Report>> GetReportsByStatusAsync(string status)
        => await _unitOfWork.Reports.GetByStatusAsync(status);

    public async Task<List<Report>> GetReportsByUserIdAsync(int userId)
        => await _unitOfWork.Reports.GetByUserIdAsync(userId);

    

    public async Task<bool> UpdateReportAsync(Report report)
    {
        try
        {
            _unitOfWork.Reports.PrepareUpdate(report);
            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateReportStatusAsync(int id, string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;

        var normalized = status.Trim().ToLower();
        if (!AllowedStatuses.Contains(normalized)) return false;

        var entity = await _unitOfWork.Reports.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = normalized;
        _unitOfWork.Reports.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

    

    public async Task<bool> DeleteReportAsync(int id)
    {
        var entity = await _unitOfWork.Reports.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "rejected";
        _unitOfWork.Reports.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

   

    public async Task<List<Report>> SearchReportsByYearAsync(int year, string? type, string? description, int? userId)
        => await _unitOfWork.Reports.SearchByYearAsync(year, type, description, userId);
}