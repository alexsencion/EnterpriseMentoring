using System;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Api.Domain.Entites;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }

    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
