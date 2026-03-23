using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Repositories.UnitOfWork;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChickenBarnModel = AutoFeed_Backend_DAO.Models.ChickenBarn;

namespace AutoFeed_Backend_Services.Services;

public class ChickenBarnService : IChickenBarnService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChickenBarnService()
    {
        _unitOfWork = new UnitOfWork();
    }

    public ChickenBarnService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateAsync(ChickenBarnModel entity)
    {
        _unitOfWork.ChickenBarns.PrepareCreate(entity);
        return await _unitOfWork.SaveChangesWithTransactionAsync();
    }

    public async Task<ChickenBarnModel?> GetByIdAsync(int id)
    {
        return await _unitOfWork.ChickenBarns.GetByIdAsync(id);
    }

    public async Task<List<ChickenBarnModel>> GetAllAsync()
    {
        return await _unitOfWork.ChickenBarns.GetAllAsync();
    }

    public async Task<bool> UpdateAsync(ChickenBarnModel entity)
    {
        try
        {
            _unitOfWork.ChickenBarns.PrepareUpdate(entity);
            var r = await _unitOfWork.SaveChangesWithTransactionAsync();
            return r > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.ChickenBarns.GetByIdAsync(id);
        if (entity == null) return false;
        entity.Status = false;
        _unitOfWork.ChickenBarns.PrepareUpdate(entity);
        var r = await _unitOfWork.SaveChangesWithTransactionAsync();
        return r > 0;
    }

    public async Task<List<ChickenBarnModel>> SearchAsync(int? barnId, int? flockId, int? chickenLid, bool includeInactive = false)
    {
        return await _unitOfWork.ChickenBarns.SearchAsync(barnId, flockId, chickenLid, includeInactive);
    }
}
