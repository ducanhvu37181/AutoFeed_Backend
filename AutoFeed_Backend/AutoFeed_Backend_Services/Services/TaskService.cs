using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Repositories.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskModel = AutoFeed_Backend_DAO.Models.Task;

namespace AutoFeed_Backend_Services.Services;

public class TaskService : ITaskService
{
    private readonly TaskRepository _repo;

    public TaskService()
    {
        _repo = new TaskRepository();
    }

    public TaskService(TaskRepository repository)
    {
        _repo = repository ?? new TaskRepository();
    }

    // Create
    public async Task<int> CreateTaskAsync(TaskModel task)
    {
        return await _repo.CreateAsync(task);
    }

    // Read
    public async Task<TaskModel?> GetTaskByIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<List<TaskModel>> GetAllTasksAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<List<TaskModel>> GetActiveTasksAsync()
    {
        return await _repo.getActiveTaskAsync();
    }

    public async Task<List<TaskModel>> GetInactiveTasksAsync()
    {
        return await _repo.getInactiveTaskAsync();
    }

    // Update
    public async Task<bool> UpdateTaskAsync(TaskModel task)
    {
        try
        {
            var result = await _repo.UpdateAsync(task);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    // Delete -> soft delete: set Status = false
    public async Task<bool> DeleteTaskAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        entity.Status = false;
        return await UpdateTaskAsync(entity);
    }
}
