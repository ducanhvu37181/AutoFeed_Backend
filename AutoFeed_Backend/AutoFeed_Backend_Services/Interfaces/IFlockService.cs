using AutoFeed_Backend.Models.Requests.Flock;
using AutoFeed_Backend.Models.Responses;     
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces
{
    public interface IFlockService
    {
        Task<IEnumerable<FlockResponse>> GetAllFlocksAsync();
        Task<IEnumerable<FlockResponse>> GetFlocksDashboardAsync(string? searchTerm, int? barnId, string? status);
        Task<FlockResponse?> GetFlockByIdAsync(int id);
        /// <summary>Returns new flock id on success, null on failure.</summary>
        Task<int?> CreateFlockAsync(FlockCreateRequest request);
        Task<bool> UpdateFlockAsync(FlockUpdateRequest request);
        /// <returns>Success + null error, or failure + message (FK / DB).</returns>
        Task<(bool Success, string? Error)> DeleteFlockAsync(int id);
        Task<bool> UpgradeToLargeChickenAsync(FlockUpgradeRequest request);
    }
}