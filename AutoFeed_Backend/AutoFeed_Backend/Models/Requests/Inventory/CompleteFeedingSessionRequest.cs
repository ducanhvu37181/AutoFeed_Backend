namespace AutoFeed_Backend.Models.Requests.Inventory
{
    public class CompleteFeedingSessionRequest
    {
        public int SessionId { get; set; }
        public decimal ActualQuantity { get; set; }
    }
}
