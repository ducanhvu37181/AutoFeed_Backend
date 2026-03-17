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

    Task<List<ScheduleModel>> GetInProgressTaskAsync();
    Task<List<ScheduleModel>> GetCompletedTaskAsync();
    Task<List<ScheduleModel>> SearchSchedulesAsync(string query);
}
