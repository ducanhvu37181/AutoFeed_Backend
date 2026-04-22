namespace AutoFeed_Backend.Models.Requests.Food
{
    // Class để tạo mới (Không cần ID)
    public class FoodCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Note { get; set; }
    }

    public class FoodUpdateRequest
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Note { get; set; }
    }
}