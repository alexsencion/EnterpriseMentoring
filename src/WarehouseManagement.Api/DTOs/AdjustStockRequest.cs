using System;

namespace WarehouseManagement.Api.DTOs;

public class AdjustStockRequest
{
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
