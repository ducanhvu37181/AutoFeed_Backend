using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class BarnService : IBarnService
{
    private readonly IUnitOfWork _unitOfWork;

    public BarnService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Barn>> GetAllAsync() => await _unitOfWork.Barns.GetAllAsync();

    public async Task<Barn?> GetByIdAsync(int id) => await _unitOfWork.Barns.GetByIdAsync(id);

    public async Task<bool> CreateBarnAsync(Barn barn)
    {
        _unitOfWork.Barns.PrepareCreate(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> UpdateBarnAsync(Barn barn)
    {
        _unitOfWork.Barns.PrepareUpdate(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }

    public async Task<bool> DeleteBarnAsync(int id)
    {
        var barn = await _unitOfWork.Barns.GetByIdAsync(id);
        if (barn == null) return false;

        _unitOfWork.Barns.PrepareRemove(barn);
        return await _unitOfWork.SaveChangesWithTransactionAsync() > 0;
    }
}