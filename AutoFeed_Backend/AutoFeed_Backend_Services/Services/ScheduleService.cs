using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Repositories.UnitOfWork;
namespace AutoFeed_Backend_Services.Services;
public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _unitOfWork;
    public ScheduleService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
}