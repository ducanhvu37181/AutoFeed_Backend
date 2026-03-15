using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AutoFeedDBContext _context;
    private TaskRepository _taskRepository;
    private LargeChickenRepository _largeChickenRepository;
    private UserRepository _userRepository;
    private FoodRepository _foodRepository;

    public UnitOfWork() => _context ??= new AutoFeedDBContext();

    public UnitOfWork(AutoFeedDBContext context)
    {
        _context = context;
    }

    public TaskRepository Tasks
    {
        get { return _taskRepository ??= new TaskRepository(_context); }
    }
    public LargeChickenRepository LargeChickens
    {
        get { return _largeChickenRepository ??= new LargeChickenRepository(_context); }
    }
    public UserRepository Users
    {
        get { return _userRepository ??= new UserRepository(_context); }
    }

    public FoodRepository Foods
    {
        get { return _foodRepository ??= new FoodRepository(_context); }
    }

    public int SaveChangesWithTransaction()
    {
        int result = 0;
        using (var dbContextTransaction = _context.Database.BeginTransaction())
        {
            try
            {
                result = _context.SaveChanges();
                dbContextTransaction.Commit();
            }
            catch (System.Exception ex)
            {
                result = 0;
                dbContextTransaction.Rollback();
            }
        }
        return result;
    }

    public async Task<int> SaveChangesWithTransactionAsync()
    {
        int result = 0;
        using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                result = await _context.SaveChangesAsync();
                await dbContextTransaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                result = 0;
                await dbContextTransaction.RollbackAsync();
            }
        }
        return result;
    }
}