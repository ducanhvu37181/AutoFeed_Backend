using System.Threading.Tasks;
using AutoFeed_Backend_Repositories.Repositories;

namespace AutoFeed_Backend_Repositories.UnitOfWork;

public interface IUnitOfWork
{
    TaskRepository Tasks { get; }
    LargeChickenRepository LargeChickens { get; }
    UserRepository Users { get; }
    FoodRepository Foods { get; }
    IoTDeviceRepository IoTDevices { get; } 

    int SaveChangesWithTransaction();
    Task<int> SaveChangesWithTransactionAsync();
}