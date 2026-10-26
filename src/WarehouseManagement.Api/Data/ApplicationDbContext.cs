using System;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Api.Domain.Entites;

namespace WarehouseManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) {}

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockAdjustmentLog> StockAdjustments => Set<StockAdjustmentLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
