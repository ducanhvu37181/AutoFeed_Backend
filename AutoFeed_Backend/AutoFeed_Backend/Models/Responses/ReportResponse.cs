namespace AutoFeed_Backend.Models.Responses
{   public class ReportResponse
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }      // pending | approved | rejected
        public DateTime? CreateDate { get; set; }
    }
}
