using System;

namespace AutoFeed_Backend.Models.Requests.ChickenBarn;

public class UpdateChickenBarnRequest
{
    public DateOnly? ExportDate { get; set; }
    public string? Note { get; set; }
    // Accept values: "active", "exported", "inactive"
    public string? Status { get; set; }
}
