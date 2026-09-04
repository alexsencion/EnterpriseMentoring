using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WarehouseManagement.Api.Domain.Entites;
using WarehouseManagement.Api.DTOs;
using WarehouseManagement.Api.Services;

namespace WarehouseManagement.Tests;

public class ProductServiceTests : IClassFixture<SqliteTestFixture>
{
    private readonly SqliteTestFixture _fixture;
    private readonly Mock<ILogger<ProductService>> _loggerMock;

    public ProductServiceTests(SqliteTestFixture fixture)
    {
        _fixture = fixture;
        _loggerMock = new Mock<ILogger<ProductService>>();
    }

    [Fact]
    public async Task IncreaseStockAsync_WithPositiveNumber_IncreaseProductStock()
    {
        // Arrange
        await using var context = _fixture.CreateContext();

         context.Products.Add(new Product
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

        // Assert
        var product = await context.Products.FindAsync(1);

        Assert.NotNull(product);
        Assert.Equal(15, product.StockQuantity);
    }

    [Fact]
    public async Task IncreaseStockAsync_WithNegativeNumberOrZero_ShouldThrowArgumentException()
    {
        // Arrange
        await using var context = _fixture.CreateContext();

         context.Products.Add(new Product
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
        await using var context = _fixture.CreateContext();

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
        await using var context = _fixture.CreateContext();

        var products = new List<Product>
        {
            new Product {Id = 3, Name = "Product 03", Sku = "Sku-003", StockQuantity = 30},
            new Product {Id = 4, Name = "Product 04", Sku = "Sku-004", StockQuantity = 40},
            new Product {Id = 5, Name = "Product 05", Sku = "Sku-005", StockQuantity = 50},
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var service = new ProductService(context, _loggerMock.Object);

        var query = new ProductQueryParameters
        {
            SearchTerm = "Product 05"
        };

        CancellationToken cancellationToken = default;

        var result = await service.GetAllAsync(query, cancellationToken);

        var product = result.Data.FirstOrDefault(product => product.Name == query.SearchTerm);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Product 05", product?.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_FiltersCorrectly()
    {
        await using var context = _fixture.CreateContext();

        for (int i = 1; i <= 15; i++)
        {
            context.Products.Add(new Product
            {
                Id = i,
                Name = $"Product {i:D2}",
                Sku = $"SKU-{i:D3}",
                StockQuantity = 10
            });
        }
        await context.SaveChangesAsync();

        var service = new ProductService(context, _loggerMock.Object);

        var query = new ProductQueryParameters
        {
            PageNumber = 2,
            PageSize = 5
        };

        CancellationToken cancellationToken = default;

        var result = await service.GetAllAsync(query, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);

        Assert.Equal(5, result.Data.Count());

        var itemIds = result.Data.Select(p => p.ProductId).ToList();
        Assert.Equal(new[] { 6, 7, 8, 9, 10 }, itemIds);
    }
}

        // REMEMBER TO REFACTOR ALL THESE TESTS TO FOCUS ONLY ON CORE FUNCTIONALITY AND BUSINESS RULES.
