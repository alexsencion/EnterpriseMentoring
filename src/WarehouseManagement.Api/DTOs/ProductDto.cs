using System;

namespace WarehouseManagement.Api.DTOs;

public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
