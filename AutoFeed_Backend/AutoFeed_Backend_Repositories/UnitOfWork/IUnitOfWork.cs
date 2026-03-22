using System.Threading.Tasks;
using AutoFeed_Backend_Repositories.Repositories;

namespace AutoFeed_Backend_Repositories.UnitOfWork;

public interface IUnitOfWork
{
    TaskRepository Tasks { get; }
    LargeChickenRepository LargeChickens { get; }
    UserRepository Users { get; }
    FoodRepository Foods { get; }
    ScheduleRepository Schedules { get; }
    IoTDeviceRepository IoTDevices { get; }
    RequestRepository Requests { get; }

    int SaveChangesWithTransaction();
    Task<int> SaveChangesWithTransactionAsync();
}