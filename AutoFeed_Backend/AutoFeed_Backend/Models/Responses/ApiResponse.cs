namespace AutoFeed_Backend.Models.Responses;

public class ApiResponse<T>
{
    public bool Status { get; set; }
    public int HttpCode { get; set; }
    public T Data { get; set; }
    public string Description { get; set; }
}
