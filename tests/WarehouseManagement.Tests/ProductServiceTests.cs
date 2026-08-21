using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WarehouseManagement.Api.Data;
using WarehouseManagement.Api.Domain.Entites;
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
                Name = "Product 01",
                Sku = "Sku-001",
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

         context.Products.Add(new Product
            {
                Id = 3,
                Name = "Product 01",
                Sku = "Sku-001",
                StockQuantity = 10
            });

            await context.SaveChangesAsync();

        var service = new ProductService(context, _loggerMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<NullReferenceException>(async () => await service.IncreaseStockAsync(2, new ProductStockRequest
        {
            StockQuantity = 3
        }));

        // Assert
        Assert.Contains("The product doesn't exist", exception.Message);
    }
}
