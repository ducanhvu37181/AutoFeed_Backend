using System.Threading.Tasks;
using AutoFeed_Backend_DAO.Models;
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
    FeedingRuleRepository FeedingRules { get; }
    IoTDeviceRepository IoTDevices { get; }
    RequestRepository Requests { get; }
    BarnRepository Barns { get; }
    InventoryRepository Inventories { get; }
    ErrorIoTRepository ErrorIoTs { get; }
    BarnImageRepository BarnImages { get; }

    ReportRepository Reports { get; }
    DataIoTRepository DataIoTs { get; }
    NotificationRepository Notifications { get; }

    int SaveChangesWithTransaction();
    Task<int> SaveChangesWithTransactionAsync();
    System.Threading.Tasks.Task<Barn?> GetBarnByDeviceIdAsync(int deviceId);
}