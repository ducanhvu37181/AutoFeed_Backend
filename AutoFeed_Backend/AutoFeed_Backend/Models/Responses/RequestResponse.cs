namespace AutoFeed_Backend.Models.Responses
{
    public class RequestResponse
    {
        public int RequestId { get; set; }
        public int? UserId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}
