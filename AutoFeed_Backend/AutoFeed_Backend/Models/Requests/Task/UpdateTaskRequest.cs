namespace AutoFeed_Backend.Models.Requests.Task;

public class UpdateTaskRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool? Status { get; set; }
}
