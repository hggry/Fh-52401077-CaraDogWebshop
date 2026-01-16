using CaraDog.Core.Abstractions.Services;
using CaraDog.DTO.Carts;
using Microsoft.AspNetCore.Mvc;

namespace CaraDog.Api.Controllers;

[ApiController]
[Route("api/cart")]
public sealed class CartController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<CartController> _logger;

    public CartController(IOrderService orderService, ILogger<CartController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost("info")]
    public async Task<ActionResult<CartInfoDto>> GetCartInfo(CartInfoRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("HBH-API-CART-001 Get cart info");
        var result = await _orderService.GetCartInfoAsync(request, cancellationToken);
        return Ok(result);
    }
}
