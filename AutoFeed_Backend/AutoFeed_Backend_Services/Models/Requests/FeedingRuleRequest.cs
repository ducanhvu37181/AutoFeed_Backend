using System;

namespace AutoFeed_Backend_Services.Models.Requests.FeedingRuleRequest
{
    // DTO để tạo mới Rule gốc
    public class FeedingRuleCreateDto
    {
        public int? ChickenLid { get; set; }
        public int? FlockId { get; set; }
        public int Times { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Description { get; set; } = null!;
        public string? Note { get; set; }
    }

    // DTO để Cập nhật Rule gốc 
    public class FeedingRuleUpdateDto
    {
        public int? ChickenLid { get; set; }
        public int? FlockId { get; set; }
        public int Times { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Description { get; set; } = null!;
        public string? Note { get; set; }
    }

    // DTO để thêm bữa ăn mới (Detail)
    public class RuleDetailCreateDto
    {
        public int RuleID { get; set; }
        public int FoodID { get; set; }        
        public decimal Amount { get; set; }
        public int FeedHour { get; set; }
        public int FeedMinute { get; set; }
        public string Description { get; set; } = null!;
    }

    // DTO để cập nhật bữa ăn (Detail)
    public class RuleDetailUpdateDto
    {
        public int FoodID { get; set; }
        public decimal Amount { get; set; }
        public int FeedHour { get; set; }
        public int FeedMinute { get; set; }
        public string Description { get; set; } = null!;
    }
}