using AutoFeed_Backend_DAO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IRequestService
{
    // Create
    Task<int> CreateRequestAsync(Request request);

    // Read
    Task<Request?> GetRequestByIdAsync(int id);
    Task<List<Request>> GetAllRequestsAsync();
    Task<List<Request>> GetActiveRequestsAsync();
    Task<List<Request>> GetInactiveRequestsAsync();

    // Update
    Task<bool> UpdateRequestAsync(Request request);

    // Delete (soft delete -> Status = false)
    Task<bool> DeleteRequestAsync(int id);

    // Search
    Task<List<Request>> SearchRequestsAsync(string query);
}