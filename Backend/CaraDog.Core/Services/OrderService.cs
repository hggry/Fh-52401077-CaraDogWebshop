using CaraDog.Core.Abstractions.Email;
using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Abstractions.Tax;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Mappers;
using CaraDog.Db;
using CaraDog.Db.Entities;
using DbOrderStatus = CaraDog.Db.Enums.OrderStatus;
using CaraDog.DTO.Enums;
using CaraDog.DTO.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Services;

public sealed class OrderService : IOrderService
{
    private const decimal ShippingFlatRate = 7.99m;
    private const decimal FreeShippingThreshold = 75.00m;

    private readonly CaraDogDbContext _dbContext;
    private readonly ITaxCalculatorResolver _taxResolver;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        CaraDogDbContext dbContext,
        ITaxCalculatorResolver taxResolver,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _taxResolver = taxResolver;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return orders.Select(order => order.ToDto()).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order {id} was not found.");
        }

        return order.ToDto();
    }

    public async Task<OrderDto> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException("Order must contain at least one item.");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.Customer.FirstName.Trim(),
            LastName = request.Customer.LastName.Trim(),
            Email = request.Customer.Email.Trim(),
            Phone = request.Customer.Phone?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var address = new Address
        {
            Id = Guid.NewGuid(),
            Street = request.ShippingAddress.Street.Trim(),
            City = request.ShippingAddress.City.Trim(),
            PostalCode = request.ShippingAddress.PostalCode.Trim(),
            CountryCode = request.ShippingAddress.CountryCode.Trim().ToUpperInvariant(),
            State = request.ShippingAddress.State?.Trim()
        };

        var productIds = request.Items.Select(item => item.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Include(p => p.Inventory)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new ValidationException("One or more products do not exist.");
        }

        // Umsatzgrenze: aktuell immer AT 20% verwenden, unabhängig vom Land.
        var taxCalculator = _taxResolver.Resolve("AT");

        var orderItems = new List<OrderItem>();
        decimal subtotalNet = 0;
        decimal taxAmount = 0;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ValidationException("Item quantity must be greater than zero.");
            }

            var product = products.First(p => p.Id == item.ProductId);
            var inventory = product.Inventory;
            var available = inventory?.Quantity ?? 0;

            if (available < item.Quantity)
            {
                throw new ValidationException($"Insufficient stock for product {product.Id}.");
            }

            var lineNet = Math.Round(product.NetPrice * item.Quantity, 2, MidpointRounding.AwayFromZero);
            var lineTax = taxCalculator.CalculateTax(lineNet);
            subtotalNet += lineNet;
            taxAmount += lineTax;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                Quantity = item.Quantity,
                UnitNetPrice = product.NetPrice,
                LineNet = lineNet,
                TaxAmount = lineTax,
                LineTotalGross = lineNet + lineTax
            });

            inventory!.Quantity -= item.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            product.IsSoldOut = inventory.Quantity <= 0;
        }

        var shippingCost = subtotalNet >= FreeShippingThreshold ? 0 : ShippingFlatRate;
        var totalGross = subtotalNet + taxAmount + shippingCost;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Customer = customer,
            ShippingAddressId = address.Id,
            ShippingAddress = address,
            CreatedAt = DateTime.UtcNow,
            Status = DbOrderStatus.PaymentPending,
            PaymentProvider = EntityDtoMapper.MapProviderDto(request.PaymentProvider),
            SubtotalNet = subtotalNet,
            TaxAmount = taxAmount,
            ShippingCost = shippingCost,
            TotalGross = totalGross,
            Items = orderItems
        };

        _dbContext.Customers.Add(customer);
        _dbContext.Addresses.Add(address);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var orderDto = order.ToDto();
        try
        {
            await _emailService.SendOrderConfirmationAsync(orderDto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HBH-ORD-004 Failed to send confirmation email for {OrderId}", order.Id);
        }

        _logger.LogInformation("HBH-ORD-001 Order created {OrderId}", order.Id);

        return orderDto;
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order {id} was not found.");
        }

        order.Status = EntityDtoMapper.MapStatusDto(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-ORD-002 Order status updated {OrderId} {Status}", order.Id, order.Status);

        return order.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order {id} was not found.");
        }

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-ORD-003 Order deleted {OrderId}", id);
    }

    private static void ValidateRequest(OrderCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Customer.FirstName))
        {
            throw new ValidationException("Customer first name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Customer.LastName))
        {
            throw new ValidationException("Customer last name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Customer.Email))
        {
            throw new ValidationException("Customer email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ShippingAddress.Street))
        {
            throw new ValidationException("Shipping street is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ShippingAddress.City))
        {
            throw new ValidationException("Shipping city is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ShippingAddress.PostalCode))
        {
            throw new ValidationException("Shipping postal code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ShippingAddress.CountryCode))
        {
            throw new ValidationException("Shipping country code is required.");
        }
    }
}
