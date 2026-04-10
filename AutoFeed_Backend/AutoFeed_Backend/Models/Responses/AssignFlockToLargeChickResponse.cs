using System;
namespace AutoFeed_Backend.Models.Responses
{
	public class AssignFlockToLargeChickResponse
	{
		public int flockId { get; set; }
		public int ChickenLid { get; set; }
		public string ChickenLName { get; set; }
        public int barnId { get; set; }
		public string CBarnStatus { get; set; }
		public DateOnly StartDate { get; set; }
    }
}

