#nullable disable
using System;
using System.Collections.Generic;

namespace AutoFeed_Backend_DAO.Models;

public partial class FeedingGuideFlock
{
    public int GuideFid { get; set; }

    public string ChickenType { get; set; }

    public string Status { get; set; }

    public int Age { get; set; }

    public decimal FeedPerDay { get; set; }

    public int Session { get; set; }

    public string Note { get; set; }

    public DateTime? CreatedAt { get; set; }
}
