using System;

namespace AutoFeed_Backend.Models.Responses
{
    public class FlockResponse
    {
        public int FlockId { get; set; }
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        public DateTime DoB { get; set; }
        public string HealthStatus { get; set; } = null!;
        public string? Note { get; set; }
    }
}