using System;
using System.Collections.Generic;

namespace AutoFeed_Backend_Services.Models.Responses
{
    public class RuleDetailResponse
    {
        public int FeedRuleDetailID { get; set; }
        public int FoodID { get; set; }
        public string FoodName { get; set; } = null!;
        public int FeedHour { get; set; }
        public int FeedMinute { get; set; }
        public bool Status { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    public class FeedingRuleFullResponse
    {
        public int RuleId { get; set; }
        public string Description { get; set; } = null!;
        public int Times { get; set; }
        public List<RuleDetailResponse> Details { get; set; } = new();
    }
}