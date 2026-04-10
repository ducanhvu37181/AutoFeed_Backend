using System;

namespace AutoFeed_Backend.Models.Requests
{
    public class AddDataIoTRequest
    {
        public int BarnId { get; set; }
        public int DeviceId { get; set; }
        public decimal Value { get; set; }
        public string? Description { get; set; }
    }
}
