namespace AutoFeed_Backend.Models.Requests.Flock;

public class TransferQuantityToFlockRequest
{
    public int SourceFlockId { get; set; }
    public int TargetFlockId { get; set; }
}
