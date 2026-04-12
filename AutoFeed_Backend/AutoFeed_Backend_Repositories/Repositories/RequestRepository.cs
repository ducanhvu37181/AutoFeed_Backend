using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories;

public class RequestRepository : GenericRepository<Request>
{
    public RequestRepository() : base() { }

    public RequestRepository(AutoFeedDBContext context) : base(context) { }

    public async Task<List<Request>> GetActiveAsync()
    {
        // Deprecated: use GetByStatusAsync
        return await _context.Set<Request>()
            .Where(r => r.Status != null && (r.Status.ToLower() == "pending" || r.Status.ToLower() == "approved"))
            .ToListAsync();
    }

    public async Task<List<Request>> GetInactiveAsync()
    {
        // Deprecated: use GetByStatusAsync
        return await _context.Set<Request>()
            .Where(r => r.Status != null && r.Status.ToLower() == "rejected")
            .ToListAsync();
    }

    public async Task<List<Request>> GetByUserIdAsync(int userId)   
    {
        return await _context.Set<Request>()
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Request>> GetByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return new List<Request>();

        var s = status.Trim().ToLower();
        return await _context.Set<Request>()
            .Where(r => r.Status != null && r.Status.ToLower() == s)
            .ToListAsync();
    }

    // Search by id, type, description
    public async Task<List<Request>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await _context.Set<Request>().ToListAsync();

        query = query.Trim();

        if (int.TryParse(query, out var id))
        {
            return await _context.Set<Request>()
                .Where(r => r.RequestId == id
                    || (r.Type != null && r.Type.Contains(query))
                    || (r.Description != null && r.Description.Contains(query)))
                .ToListAsync();
        }

        return await _context.Set<Request>()
            .Where(r => (r.Type != null && r.Type.Contains(query))
                     || (r.Description != null && r.Description.Contains(query)))
            .ToListAsync();
    }
}