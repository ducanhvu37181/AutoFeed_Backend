namespace AutoFeed_Backend_Services.Models.Requests
{
    public class DeviceCreateRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class DeviceUpdateRequest
    {
        public int DeviceID { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool Status { get; set; }
    }

    public class DeviceAssignRequest
    {
        public int BarnID { get; set; }
    }
}