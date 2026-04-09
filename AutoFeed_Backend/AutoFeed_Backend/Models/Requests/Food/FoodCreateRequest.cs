namespace AutoFeed_Backend.Models.Requests.Food
{
    // Class để tạo mới (Không cần ID)
    public class FoodCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Note { get; set; }
    }

    // Class để cập nhật (BẮT BUỘC CÓ ID) - Kiên thêm đoạn này nhé
    public class FoodUpdateRequest
    {
        public int FoodId { get; set; } // Phải có cái này để hết lỗi Build
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Note { get; set; }
    }
}