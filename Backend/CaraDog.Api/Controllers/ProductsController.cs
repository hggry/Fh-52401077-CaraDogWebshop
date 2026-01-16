using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Products;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-PRD-001 Get products");
        var result = await _productService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-sku")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetBySku([FromQuery] string? skus, CancellationToken cancellationToken)
    {
        var skuList = SplitQueryValues(skus);
        if (skuList.Count == 0)
        {
            return BadRequest("At least one sku is required.");
        }

        _logger.LogInformation("HBH-API-PRD-002 Get products by sku {SkuCount}", skuList.Count);
        var result = await _productService.GetBySkuAsync(skuList, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-category")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetByCategory([FromQuery] string? categories, CancellationToken cancellationToken)
    {
        var categoryList = SplitQueryValues(categories);
        if (categoryList.Count == 0)
        {
            return BadRequest("At least one category is required.");
        }

        _logger.LogInformation("HBH-API-PRD-003 Get products by category {CategoryCount}", categoryList.Count);
        var result = await _productService.GetByCategoryAsync(categoryList, cancellationToken);
        return Ok(result);
    }

    private static IReadOnlyList<string> SplitQueryValues(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }
}
