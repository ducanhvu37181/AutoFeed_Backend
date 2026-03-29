using System.Threading.Tasks;
using AutoFeed_Backend_Repositories.Repositories;

namespace AutoFeed_Backend_Repositories.UnitOfWork;

public interface IUnitOfWork
{
    FlockRepository Flocks { get; } 
    TaskRepository Tasks { get; }
    LargeChickenRepository LargeChickens { get; }
    UserRepository Users { get; }
    FoodRepository Foods { get; }
    ScheduleRepository Schedules { get; }
    ChickenBarnRepository ChickenBarns { get; }
    IoTDeviceRepository IoTDevices { get; }
    RequestRepository Requests { get; }
    BarnRepository Barns { get; }
    InventoryRepository Inventories { get; }
    ReportRepository Reports { get; }

    int SaveChangesWithTransaction();
    Task<int> SaveChangesWithTransactionAsync();
}