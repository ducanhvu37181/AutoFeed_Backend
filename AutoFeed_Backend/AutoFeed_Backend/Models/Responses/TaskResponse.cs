namespace AutoFeed_Backend.Models.Responses;

public class TaskResponse
{
    public int TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool? Status { get; set; }
}
