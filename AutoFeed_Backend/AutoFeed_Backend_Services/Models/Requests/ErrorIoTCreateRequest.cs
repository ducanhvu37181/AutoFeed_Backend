namespace AutoFeed_Backend_Services.Models.Requests;

public class ErrorIoTCreateRequest
{
    public int DeviceId { get; set; }

    public int BarnId { get; set; }

    public string ErrorMessage { get; set; }

    public string Severity { get; set; }
}
