namespace AutoFeed_Backend.Models.Requests.Request
{
    public class UpdateRequestRequest
    {
        public int? UserId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        // status values: "pending", "approved", "rejected"
        public string Status { get; set; }
    }
}
