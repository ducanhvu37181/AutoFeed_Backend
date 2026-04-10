using System;

namespace AutoFeed_Backend.Models.Responses
{
    public class AddDataIoTResponse
    {
        public int BarnId { get; set; }
        public string DeviceName { get; set; } = null!;
        public decimal Value { get; set; }
        public string? Description { get; set; }
        public DateTime RecordDate { get; set; }
        public int SequenceNumber { get; set; }
    }
}
