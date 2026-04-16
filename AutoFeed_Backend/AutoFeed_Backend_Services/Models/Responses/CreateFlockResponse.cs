namespace AutoFeed_Backend_Services.Models.Responses;

public class CreateFlockResponse
{
    public int FlockId { get; set; }
    public int? AssignedBarnId { get; set; }
    public string? AssignedBarnName { get; set; }
    public string AssignmentStatus { get; set; } = "assigned"; // "assigned" or "no_barn_available"
}
