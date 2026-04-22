#nullable disable
using System;
using System.Collections.Generic;

namespace AutoFeed_Backend_DAO.Models;

public partial class InventoryHistory
{
    public int HistoryId { get; set; }

    public int InventId { get; set; }

    public string FoodName { get; set; }

    public int OldQuantity { get; set; }

    public int NewQuantity { get; set; }

    public int QuantityChange { get; set; }

    public decimal WeightPerBag { get; set; }

    public DateOnly ImportDate { get; set; }

    public DateOnly ExpiredDate { get; set; }

    public DateTime ChangedAt { get; set; }

    public int? ChangedBy { get; set; }

    public string ActionType { get; set; }

    public virtual Inventory Inventory { get; set; }

    public virtual User ChangedByUser { get; set; }
}
