using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Api.Domain.Entites;
using WarehouseManagement.Api.DTOs;
using WarehouseManagement.Api.Services;

namespace WarehouseManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductService service, ILogger<ProductsController> logger)
        {
            _service = service;
            _logger = logger;

        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParameters query, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAllAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{productId:int}/receive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductStockResponse>> IncreaseStock(int productId, ProductStockRequest request)
        {
            try
            {
                return await _service.IncreaseStockAsync(productId, request);
            }
            catch (NullReferenceException)
            {
                _logger.LogWarning("User tries to add stock to a non-existant product.");
                return NotFound();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }
    }
}
