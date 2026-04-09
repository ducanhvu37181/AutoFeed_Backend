namespace AutoFeed_Backend.Models.Requests.Report
{
    public class UpdateReportStatusRequest
    {
        public string Status { get; set; } = null!;   // pending | reviewed | rejected
    }
}
