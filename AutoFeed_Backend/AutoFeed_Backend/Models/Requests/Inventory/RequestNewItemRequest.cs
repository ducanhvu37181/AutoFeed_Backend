namespace AutoFeed_Backend.Models.Requests.Inventory
{
    public class RequestNewItemRequest
    {
        public int UserId { get; set; }
        public string FoodName { get; set; }
        public string Description { get; set; }
    }
}
