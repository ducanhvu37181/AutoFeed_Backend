namespace AutoFeed_Backend.Models.Requests.Task;

public class CreateTaskRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    // Accept time as string to avoid JSON parsing issues in some clients; controller will parse to TimeOnly.
    public string StartTime { get; set; }
    public string EndTime { get; set; }
}
