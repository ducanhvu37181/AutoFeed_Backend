namespace AutoFeed_Backend_DAO.Models
{
    public class FeedingRuleCreateDto
    {
        public string Category { get; set; } // Gà con, Gà chọi, hoặc Gà bệnh
        public int? ChickenLid { get; set; }
        public int? FlockId { get; set; }
        public int Times { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }
}