using System;

namespace AutoFeed_Backend.Models.Responses
{
    public class UpgradeResponse
    {
        public string Message { get; set; } = null!;
        public int NewChickenLid { get; set; }
        public int AssignedBarnId { get; set; }
        public DateTime UpgradeDate { get; set; } = DateTime.Now;
    }
}