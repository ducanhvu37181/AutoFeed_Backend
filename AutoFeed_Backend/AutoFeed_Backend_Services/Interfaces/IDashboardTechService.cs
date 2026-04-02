using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces;

public interface IDashboardTechService
{
    Task<object> GetTechnicalSummaryAsync();
    Task<IEnumerable<object>> GetUpcomingMaintenanceAsync();
    Task<object> GetFeedingConfigurationAsync();
    Task<IEnumerable<object>> GetBarnMonitoringAsync();
    Task<IEnumerable<object>> GetRecentReportsAsync();
}