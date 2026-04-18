namespace AutoFeed_Backend.Models.Requests.Inventory
{
    public class UpdateInventoryRequest
    {
        public int Quantity { get; set; }
        public string? ExpiredDate { get; set; }   // format: yyyy-MM-dd (optional)
    }
}
