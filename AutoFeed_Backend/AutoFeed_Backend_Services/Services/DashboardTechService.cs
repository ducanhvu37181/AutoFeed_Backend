using AutoFeed_Backend_Repositories.UnitOfWork;
using AutoFeed_Backend_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Services;

public class DashboardTechService : IDashboardTechService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardTechService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetTechnicalSummaryAsync()
    {
        var devices = await _unitOfWork.IoTDevices.GetAllAsync();
        var reports = await _unitOfWork.Reports.GetAllAsync();

        int total = devices.Count();
        int online = devices.Count(d => d.Status == true);

        return new
        {
            TotalDevices = total,
            Online = online,
            Offline = total - online,
            ConnectivityRate = total > 0 ? Math.Round((double)online / total * 100, 1) : 0,
            ActiveAlerts = reports.Count(r => r.Status == "pending"),
            SystemHealth = "98.5%",
            DevicesByType = devices.GroupBy(d => d.Name.Split('-')[0])
                                   .Select(g => new { Type = g.Key, Count = g.Count() })
        };
    }

    public async Task<IEnumerable<object>> GetUpcomingMaintenanceAsync()
    {
        var schedules = await _unitOfWork.Schedules.GetAllAsync();
        return schedules.Where(s => s.Status == "pending" || s.Status == "in progress")
                        .OrderBy(s => s.StartDate)
                        .Select(s => new {
                            ID = s.SchedId,
                            Task = s.Description,
                            Date = s.StartDate.ToString("yyyy-MM-dd"),
                            Priority = s.Priority
                        });
    }

    public async Task<object> GetFeedingConfigurationAsync()
    {
        return new
        {
            YoungChicken = new { Temp = "32°C", Humid = "60%", Frequency = "5x/day", Amount = "50g" },
            AdultChicken = new { Temp = "26°C", Humid = "65%", Frequency = "3x/day", Amount = "150g" }
        };
    }

    public async Task<IEnumerable<object>> GetBarnMonitoringAsync()
    {
        var barns = await _unitOfWork.Barns.GetAllAsync();
        return barns.Select(b => new {
            ID = b.BarnId,
            BarnName = b.Type,
            Temperature = b.Temperature + "°C",
            Humidity = b.Humidity + "%",
            Status = "Normal"
        });
    }

    public async Task<IEnumerable<object>> GetRecentReportsAsync()
    {
        var reports = await _unitOfWork.Reports.GetAllAsync();
        return reports.OrderByDescending(r => r.CreateDate)
                      .Take(10)
                      .Select(r => new {
                          r.ReportId,
                          r.Type,
                          r.Status,
                          Date = r.CreateDate?.ToString("yyyy-MM-dd")
                      });
    }
}