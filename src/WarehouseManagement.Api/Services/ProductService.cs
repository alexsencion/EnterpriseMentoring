using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Api.Data;
using WarehouseManagement.Api.Domain.Entites;

namespace WarehouseManagement.Api.Services;

public class ProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        return await _context.Products.Select(p => new ProductDto
        {
            ProductId = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            StockQuantity = p.StockQuantity
        }).ToListAsync();
    }

    public async Task<ProductStockResponse> IncreaseStockAsync(int productId, ProductStockRequest request)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            throw new NullReferenceException("The product doesn't exist");

        if (request.StockQuantity <= 0)
            throw new ArgumentException("The quantity must be greater than zero");
            _logger.LogWarning("User tries to add stock to a non-existant product.");

        product.StockQuantity += request.StockQuantity;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stock successfully added");
        return new ProductStockResponse
        {
            NewStockQuantity = product.StockQuantity
        };
        
    }
}
