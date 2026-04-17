namespace AutoFeed_Backend.Models.Responses;

public class CreateFlockResponse
{
    public int FlockId { get; set; }
    public int? AssignedBarnId { get; set; }
    public string? AssignedBarnName { get; set; }
}
