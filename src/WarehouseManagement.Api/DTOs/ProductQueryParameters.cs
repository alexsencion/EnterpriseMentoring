using System;

namespace WarehouseManagement.Api.DTOs;

public class ProductQueryParameters
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int ClampedPageSize => Math.Min(Math.Max(PageSize, 1), 50);
}
