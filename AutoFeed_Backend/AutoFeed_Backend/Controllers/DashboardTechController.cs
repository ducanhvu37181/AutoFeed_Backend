using AutoFeed_Backend_Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AutoFeed_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardTechController : ControllerBase
{
    private readonly IDashboardTechService _dashboardService;
    private readonly IIoTDeviceService _iotService;

    public DashboardTechController(IDashboardTechService dashboardService, IIoTDeviceService iotService)
    {
        _dashboardService = dashboardService;
        _iotService = iotService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
        => Ok(await _dashboardService.GetTechnicalSummaryAsync());

    [HttpGet("maintenance")]
    public async Task<IActionResult> GetMaintenance()
        => Ok(await _dashboardService.GetUpcomingMaintenanceAsync());

    [HttpGet("monitoring")]
    public async Task<IActionResult> GetMonitoring()
        => Ok(await _dashboardService.GetBarnMonitoringAsync());

    [HttpGet("feeding-config")]
    public async Task<IActionResult> GetFeedingConfig()
        => Ok(await _dashboardService.GetFeedingConfigurationAsync());

    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices()
        => Ok(await _iotService.GetAllDevicesAsync(null, null, null));

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports()
        => Ok(await _dashboardService.GetRecentReportsAsync());
}