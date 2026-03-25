namespace AutoFeed_Backend.Models.Requests.Request
{
    public class UpdateRequestStatusRequest   
    {
        /// <summary>Allowed values: pending, approved, rejected</summary>
        public string Status { get; set; }
    }
}
