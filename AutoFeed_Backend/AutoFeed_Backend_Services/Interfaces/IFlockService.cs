using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IFlockService
{
    Task<IEnumerable<object>> GetFlockDashboardAsync(string? searchTerm, int? barnId, string? status);
    Task<object?> GetFlockDetailAsync(int id);
    Task<bool> CreateFlockAsync(FlockChicken flock, int barnId);
    Task<bool> UpdateFlockAsync(FlockChicken flock);
    Task<bool> DeleteFlockAsync(int id);
    Task<bool> AssignIdsToLargeChickensAsync(int id);
}