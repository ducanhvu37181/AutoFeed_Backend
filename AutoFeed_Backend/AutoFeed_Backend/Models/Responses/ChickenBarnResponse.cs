using System;

namespace AutoFeed_Backend.Models.Responses;

public class ChickenBarnResponse
{
    public int CbarnId { get; set; }
    public int BarnId { get; set; }
    public int? ChickenLid { get; set; }
    public int? FlockId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? ExportDate { get; set; }
    public string Note { get; set; }
    // Status values: "active", "exported", or "inactive"
    public string Status { get; set; }
    // Avatar image URL of the chicken currently assigned to this barn (if any)
    public string? AvatarUrl { get; set; }
}
