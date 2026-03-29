using AutoFeed_Backend_DAO.Models;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IReportService
{
    
    Task<int> CreateReportAsync(Report report);
    Task<Report?> GetReportByIdAsync(int id);
    Task<List<Report>> GetAllReportsAsync();
    Task<List<Report>> GetFarmerReportsAsync();
    Task<List<Report>> GetTechFarmerReportsAsync();
    Task<List<Report>> GetReportsByStatusAsync(string status);
    Task<List<Report>> GetReportsByUserIdAsync(int userId);
    Task<bool> UpdateReportAsync(Report report);
    Task<bool> UpdateReportStatusAsync(int id, string status);
    Task<bool> DeleteReportAsync(int id);
    Task<List<Report>> SearchReportsByYearAsync(int year, string? type, string? description, int? userId);
}