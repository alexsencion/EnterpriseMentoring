using System;

namespace WarehouseManagement.Api.Domain.Entites;

public class StockAdjustmentLog
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int QuantityAdjusted { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime AdjustedAtUtc { get; set; }
}
