namespace AutoFeed_Backend.Models.Requests.Device
{
    // 1. Dùng cho POST: Đăng ký thiết bị mới
    public class DeviceCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    // 2. Dùng cho PUT: Cập nhật thông tin và trạng thái Online/Offline
    public class DeviceUpdateRequest
    {
        public int DeviceID { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool Status { get; set; } // Khớp với kiểu BIT trong Database
    }

    // 3. Dùng cho PUT: Gán thiết bị vào chuồng (Thay thế Reassign)
    public class DeviceAssignRequest
    {
        public int BarnID { get; set; }
    }
}