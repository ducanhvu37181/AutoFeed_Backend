using System.Collections.Generic;
using System.Threading.Tasks;
using ChickenBarnModel = AutoFeed_Backend_DAO.Models.ChickenBarn;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IChickenBarnService
{
    Task<int> CreateAsync(ChickenBarnModel entity);
    Task<ChickenBarnModel?> GetByIdAsync(int id);
    Task<List<ChickenBarnModel>> GetAllAsync();
    Task<List<ChickenBarnModel>> GetActiveAsync();
    Task<List<ChickenBarnModel>> GetExportedAsync();
    Task<bool> UpdateAsync(ChickenBarnModel entity);
    Task<bool> DeleteAsync(int id); // soft delete
    Task<List<ChickenBarnModel>> SearchAsync(int? barnId, int? flockId, int? chickenLid, bool includeInactive = false);
}
