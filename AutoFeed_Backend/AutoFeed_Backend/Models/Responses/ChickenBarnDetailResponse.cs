using System;

namespace AutoFeed_Backend.Models.Responses;

public class ChickenBarnDetailResponse
{
    public int CbarnId { get; set; }
    public int BarnId { get; set; }
    public string BarnType { get; set; }
    public string? ChickenName { get; set; }
    public string? FlockName { get; set; }
    public string Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? ExportDate { get; set; }
    public DateOnly? TransferDate { get; set; }
    public string? AvatarUrl { get; set; }
}
