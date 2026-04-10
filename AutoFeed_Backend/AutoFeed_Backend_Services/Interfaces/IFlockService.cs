using AutoFeed_Backend.Models.Requests.Flock;
using AutoFeed_Backend.Models.Responses;     
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces
{
    public interface IFlockService
    {
        Task<IEnumerable<FlockResponse>> GetAllFlocksAsync();
        Task<FlockResponse?> GetFlockByIdAsync(int id);
        Task<bool> CreateFlockAsync(FlockCreateRequest request);
        Task<bool> UpdateFlockAsync(FlockUpdateRequest request);
        Task<bool> DeleteFlockAsync(int id);
        Task<bool> UpgradeToLargeChickenAsync(FlockUpgradeRequest request);
    }
}