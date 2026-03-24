using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class RequestService : IRequestService
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestService()
    {
        _unitOfWork = new UnitOfWork();
    }

    public RequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Create
    public async Task<int> CreateRequestAsync(Request request)
    {
        request.Status = false;                          // mặc định chưa xử lý
        request.CreatedAt = System.DateTime.UtcNow;
        _unitOfWork.Requests.PrepareCreate(request);
        return await _unitOfWork.SaveChangesWithTransactionAsync();
    }

    // Read
    public async Task<Request?> GetRequestByIdAsync(int id)
    {
        return await _unitOfWork.Requests.GetByIdAsync(id);
    }

    public async Task<List<Request>> GetAllRequestsAsync()
    {
        return await _unitOfWork.Requests.GetAllAsync();
    }

    public async Task<List<Request>> GetActiveRequestsAsync()
    {
        return await _unitOfWork.Requests.GetActiveAsync();
    }

    public async Task<List<Request>> GetInactiveRequestsAsync()
    {
        return await _unitOfWork.Requests.GetInactiveAsync();
    }

    // Update
    public async Task<bool> UpdateRequestAsync(Request request)
    {
        try
        {
            _unitOfWork.Requests.PrepareUpdate(request);
            var result = await _unitOfWork.SaveChangesWithTransactionAsync();
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    // Soft delete: Status = false
    public async Task<bool> DeleteRequestAsync(int id)
    {
        var entity = await _unitOfWork.Requests.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = false;
        _unitOfWork.Requests.PrepareUpdate(entity);
        var result = await _unitOfWork.SaveChangesWithTransactionAsync();
        return result > 0;
    }

    // Search
    public async Task<List<Request>> SearchRequestsAsync(string query)
    {
        return await _unitOfWork.Requests.SearchAsync(query);
    }
}