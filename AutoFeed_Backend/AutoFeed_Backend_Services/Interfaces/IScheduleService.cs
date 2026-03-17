using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleModel = AutoFeed_Backend_DAO.Models.Schedule;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IScheduleService
{
    Task<int> CreateScheduleAsync(ScheduleModel schedule);
    Task<ScheduleModel?> GetScheduleByIdAsync(int id);
    Task<List<ScheduleModel>> GetAllSchedulesAsync();
    Task<bool> UpdateScheduleAsync(ScheduleModel schedule);
    Task<bool> DeleteScheduleAsync(int id);

    Task<List<ScheduleModel>> GetInProgressScheduleAsync();
    Task<List<ScheduleModel>> GetCompletedScheduleAsync();
    Task<List<ScheduleModel>> GetPendingScheduleAsync();
    Task<List<ScheduleModel>> SearchSchedulesAsync(string query);

    // helper to resolve username by user id
    Task<string> GetUserNameByIdAsync(int userId);
}
