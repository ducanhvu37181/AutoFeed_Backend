#nullable disable
using System;
using System.Collections.Generic;

namespace AutoFeed_Backend_DAO.Models;

public partial class FeedingGuideLargeChicken
{
    public int GuideLid { get; set; }

    public string ChickenType { get; set; }

    public string Status { get; set; }

    public decimal Weight { get; set; }

    public decimal FeedPerDay { get; set; }

    public int Session { get; set; }

    public string Note { get; set; }

    public DateTime? CreatedAt { get; set; }
}
