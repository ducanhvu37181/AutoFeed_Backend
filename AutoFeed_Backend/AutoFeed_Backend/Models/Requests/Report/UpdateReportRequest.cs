namespace AutoFeed_Backend.Models.Requests.Report
{
    public class UpdateReportRequest
    {
        public string Type { get; set; } = null!;
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
