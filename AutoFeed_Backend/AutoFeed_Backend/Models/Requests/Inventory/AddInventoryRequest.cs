namespace AutoFeed_Backend.Models.Requests.Inventory
{
    public class AddInventoryRequest
    {
        public string FoodName { get; set; }
        public int Quantity { get; set; }
        public decimal WeightPerBag { get; set; }
        public string ExpiredDate { get; set; }   // format: yyyy-MM-dd
    }
}
