namespace AutoFeed_Backend_Services.Models.Responses;

public class FoodResponse
{
    public string FoodId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; }
    public string Note { get; set; }
}