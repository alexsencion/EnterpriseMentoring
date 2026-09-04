using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
            products = _context.Products.Where(x => x.Name.ToLower().Contains(search));
        }

        if (query.PageNumber <= 0)
            query.PageNumber = 1;

        if (query.PageSize > 50)
            query.PageSize = 50;

        var totalCount = products.Count();
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
}
