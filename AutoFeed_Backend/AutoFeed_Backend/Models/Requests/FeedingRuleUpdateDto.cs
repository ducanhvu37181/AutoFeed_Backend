using System;

namespace AutoFeed_Backend.Models.Requests
{
    public class FeedingRuleUpdateDto
    {
        public int ChickenLid { get; set; }
        public int FlockId { get; set; }
        public int Times { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
    }
}
