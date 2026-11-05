using System;
using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Api.Common;
using WarehouseManagement.Api.Data;
using WarehouseManagement.Api.Domain.Entites;
using WarehouseManagement.Api.DTOs;

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

    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParameters query, CancellationToken cancellationToken)
    {        
        IQueryable<Product> products = _context.Products;

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.ToLower();
            products = products.Where(x => x.Name.Contains(search));
        }

        if (query.PageNumber <= 0)
            query.PageNumber = 1;

        if (query.PageSize > 50)
            query.PageSize = 50;

        var totalCount = await products.CountAsync(cancellationToken);
        var pageNumber = query.PageNumber;
        var pageSize = query.ClampedPageSize;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var paged = await products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var response = paged.Select(product => new ProductDto
        {
            ProductId = product.Id,
            Name = product.Name,
            StockQuantity = product.StockQuantity
        });

        return new PagedResult<ProductDto>(response, totalCount, totalPages, pageNumber, pageSize);
    }

    public async Task<ProductStockResponse> IncreaseStockAsync(int productId, ProductStockRequest request)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId) ?? throw new NullReferenceException("The product doesn't exist");
        
        if (request.StockQuantity <= 0)
            throw new ArgumentException("The quantity must be greater than zero");

        product.StockQuantity += request.StockQuantity;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stock successfully added");
        return new ProductStockResponse
        {
            NewStockQuantity = product.StockQuantity
        };
        
    }

    public async Task<Result<ProductStockResponse>> AdjustStockAsync(int id, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning("User tries to adjust stock to a non-existant product.");
                    return Result<ProductStockResponse>.NotFound($"The product doesn't exist");
                }

                var newStock = product.StockQuantity + request.Quantity;
                if (newStock < 0)
                    return Result<ProductStockResponse>.Failure($"Stock cannot fall below zero");

                product.StockQuantity = newStock;
                product.RowVersion = Guid.NewGuid();

                await _context.StockAdjustments.AddAsync(new StockAdjustmentLog
                {
                    ProductId = product.Id,
                    QuantityAdjusted = request.Quantity,
                    Reason = request.Reason,
                    AdjustedAtUtc = DateTime.UtcNow
                }, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                return Result<ProductStockResponse>.Success(new ProductStockResponse
                {
                    NewStockQuantity = newStock
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict detected on Product ID {ProductId}. Attempt {Attempt} of {MaxRetries}.", id, attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return Result<ProductStockResponse>.Conflict("The product was modified by another user. Please try again.");
                }

                _context.ChangeTracker.Clear();

                await Task.Delay(50 * attempt, cancellationToken);
            }
        }

        return Result<ProductStockResponse>.Conflict("Could not complete the adjustment due to high contention.");
    }
}
