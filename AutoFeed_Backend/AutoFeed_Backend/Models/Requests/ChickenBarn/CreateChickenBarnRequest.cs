using System;

namespace AutoFeed_Backend.Models.Requests.ChickenBarn;

public class CreateChickenBarnRequest
{
    public int BarnId { get; set; }
    public int? ChickenLid { get; set; }
    public int? FlockId { get; set; }
    public DateOnly StartDate { get; set; }
    public string? Note { get; set; }
}
