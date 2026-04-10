namespace AutoFeed_Backend.Models.Responses
{
    public class DeviceResponse
    {
        public int DeviceID { get; set; } 
        public string DeviceName { get; set; } = null!;
        public string AssignedTo { get; set; } = null!; 
        public string Status { get; set; } = null!;     
        //public string Battery { get; set; } = "85%";
       // public string LastUpdate { get; set; } = "Just now";
    }
}