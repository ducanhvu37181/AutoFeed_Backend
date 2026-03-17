using System;

namespace AutoFeed_Backend.Models.Responses;

public class ScheduleResponse
{
    public int SchedId { get; set; }
    public int? UserId { get; set; }
    public int? TaskId { get; set; }
    public int? CbarnId { get; set; }
    public string Description { get; set; }
    public string Note { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Username { get; set; }
}
