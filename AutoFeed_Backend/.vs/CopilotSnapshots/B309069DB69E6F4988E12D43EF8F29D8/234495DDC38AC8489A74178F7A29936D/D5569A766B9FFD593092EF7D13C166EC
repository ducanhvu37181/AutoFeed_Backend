using System;

namespace AutoFeed_Backend.Models.Responses
{
    public class FlockResponse
    {
        public int FlockId { get; set; }
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        /// <summary>Date-only in JSON (no 00:00:00 time component).</summary>
        public DateOnly DoB { get; set; }
        public string HealthStatus { get; set; } = null!;
        public string? Note { get; set; }
    }
}