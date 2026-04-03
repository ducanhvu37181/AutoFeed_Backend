namespace AutoFeed_Backend.Models.Requests.Report
{
    public class CreateReportRequest
    {
        public int UserId { get; set; }
        public string Type { get; set; } = null!;
        public string? Description { get; set; }
        public string? Url { get; set; }
    }
}