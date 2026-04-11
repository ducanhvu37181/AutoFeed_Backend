using System;

namespace AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest
{
    public class FeedingRuleCreateDto
    {
        public int? ChickenLid { get; set; }
        public int? FlockId { get; set; }
        public int Times { get; set; }
        public string Description { get; set; } = null!;
        public string? Note { get; set; }
    }

    public class RuleDetailUpdateDto
    {
        public int FoodID { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int FeedHour { get; set; }
        public int FeedMinute { get; set; }
        public string Description { get; set; } = null!;
    }
}