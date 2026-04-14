namespace AutoFeed_Backend.Models.Requests.Inventory
{
    public class ConsumeInventoryRequest
    {
        public int FoodId { get; set; }
        public int Quantity { get; set; }
    }
}
