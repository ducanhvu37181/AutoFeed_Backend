using System;

namespace AutoFeed_Backend.Models.Requests.Flock
{
    public class FlockCreateRequest
    {
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        public DateTime DoB { get; set; }
        public DateTime TransferDate { get; set; }
        public string HealthStatus { get; set; } = "healthy";
        public string? Note { get; set; }
    }

    public class FlockUpdateRequest
    {
        public int FlockID { get; set; }
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        public string HealthStatus { get; set; } = null!;
        public string? Note { get; set; }
    }

    public class FlockUpgradeRequest
    {
        public int FlockID { get; set; }
        public int BarnID { get; set; }
        public string ChickenName { get; set; } = null!;
        public decimal Weight { get; set; }
        public string? Note { get; set; }
    }
}