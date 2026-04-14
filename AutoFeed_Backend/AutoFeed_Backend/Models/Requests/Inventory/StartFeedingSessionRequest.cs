namespace AutoFeed_Backend.Models.Requests.Inventory
{
    public class StartFeedingSessionRequest
    {
        public int FoodId { get; set; }
        public decimal Quantity { get; set; }
    }
}
