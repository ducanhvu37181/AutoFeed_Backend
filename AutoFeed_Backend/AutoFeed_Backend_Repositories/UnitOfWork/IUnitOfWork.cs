using System.Threading.Tasks;
using AutoFeed_Backend_Repositories.Repositories;

namespace AutoFeed_Backend_Repositories.UnitOfWork;

public interface IUnitOfWork
{
    TaskRepository Tasks { get; }

    int SaveChangesWithTransaction();
    Task<int> SaveChangesWithTransactionAsync();
}
