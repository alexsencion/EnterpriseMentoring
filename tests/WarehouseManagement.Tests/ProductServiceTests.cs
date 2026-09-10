using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WarehouseManagement.Api.Data;
using WarehouseManagement.Api.Domain.Entites;
using WarehouseManagement.Api.DTOs;
using WarehouseManagement.Api.Services;

namespace WarehouseManagement.Tests;

public class ProductServiceTests
{
    private SqliteConnection _connection = null!;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    
    public ProductServiceTests()
    {
        _loggerMock = new Mock<ILogger<ProductService>>();
    }

    [Fact]
    public async Task IncreaseStockAsync_WithPositiveNumber_IncreaseProductStock()
    {
        // Arrange
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        await context.Products.AddAsync(new Product
        {
            Id = 1,
            Name = "Product 01",
            Sku = "Sku-001",
            StockQuantity = 10
        });

        await context.SaveChangesAsync();

        var service = new ProductService(context, _loggerMock.Object);

        // Act
        await service.IncreaseStockAsync(1, new ProductStockRequest
        {
            StockQuantity = 5
        });

        var product = await context.Products.FindAsync(1);

        // Assert
        Assert.NotNull(product);
        Assert.Equal(15, product.StockQuantity);
    }

    [Fact]
    public async Task IncreaseStockAsync_WithNegativeNumberOrZero_ShouldThrowArgumentException()
    {
        // Arrange
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        await context.Products.AddAsync(new Product
        {
            Id = 2,
            Name = "Product 02",
            Sku = "Sku-002",
            StockQuantity = 10
        });

        await context.SaveChangesAsync();

        var service = new ProductService(context, _loggerMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await service.IncreaseStockAsync(2, new ProductStockRequest
        {
            StockQuantity = -5
        }));

        // Assert
        Assert.Contains("The quantity must be greater than zero", exception.Message);
    }

    [Fact]
    public async Task IncreaseStockAsync_WhenProductDoesNotExist_ThrowsNullReferenceException()
    {
        // Arrange
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var service = new ProductService(context, _loggerMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await service.IncreaseStockAsync(2, new ProductStockRequest
        {
            StockQuantity = 3
        }));

        // Assert
        Assert.Contains("The product doesn't exist", exception.Message);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersCorrectly()
    {
        // Arrange
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        for (int i = 1; i <= 3; i++)
        {
            await context.Products.AddAsync(new Product
            {
                Id = i,
                Name = $"product {i:D2}",
                Sku = $"SKU-{i:D3}",
                StockQuantity = 10
            });
        }
        await context.SaveChangesAsync();

        var service = new ProductService(context, _loggerMock.Object);

        // Act
        var query = new ProductQueryParameters
        {
            SearchTerm = "Product"
        };

        _ = query.SearchTerm.ToLower();

        CancellationToken cancellationToken = default;

        var result = await service.GetAllAsync(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal("Product", query.SearchTerm);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_FiltersCorrectly()
    {
        // Arrange
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync();

        for (int i = 1; i <= 15; i++)
        {
            await context.Products.AddAsync(new Product
            {
                Id = i,
                Name = $"Product {i:D2}",
                Sku = $"SKU-{i:D3}",
                StockQuantity = 10
            });
        }
        await context.SaveChangesAsync();

        var service = new ProductService(context, _loggerMock.Object);

         // Act
        var query = new ProductQueryParameters
        {
            PageNumber = 2,
            PageSize = 5
        };

        CancellationToken cancellationToken = default;

        var result = await service.GetAllAsync(query, cancellationToken);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);

        Assert.Equal(5, result.Data.Count());

        var itemIds = result.Data.Select(p => p.ProductId).ToList();
        Assert.Equal(new[] { 6, 7, 8, 9, 10 }, itemIds);
    }
}