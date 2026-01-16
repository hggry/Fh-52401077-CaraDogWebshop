using CaraDog.Core.Abstractions.Email;
using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Abstractions.Tax;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Mappers;
using CaraDog.Db;
using CaraDog.Db.Entities;
using DbOrderStatus = CaraDog.Db.Enums.OrderStatus;
using CaraDog.DTO.Customers;
using CaraDog.DTO.Enums;
using CaraDog.DTO.Carts;
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
            .ThenInclude(c => c.City)
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
            .ThenInclude(c => c.City)
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

        var city = await GetOrCreateCityAsync(request.Customer, cancellationToken);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.Customer.FirstName.Trim(),
            LastName = request.Customer.LastName.Trim(),
            Email = request.Customer.Email.Trim(),
            Phone = request.Customer.Phone?.Trim(),
            Street = request.Customer.Street.Trim(),
            HouseNumber = request.Customer.HouseNumber.Trim(),
            AddressLine2 = request.Customer.AddressLine2?.Trim(),
            CityId = city.Id,
            City = city,
            CreatedAt = DateTime.UtcNow
        };

        var skus = request.Items
            .Select(item => item.Sku.Trim())
            .Where(sku => !string.IsNullOrWhiteSpace(sku))
            .Select(sku => sku.ToUpperInvariant())
            .Distinct()
            .ToList();
        var products = await _dbContext.Products
            .Include(p => p.Inventory)
            .Where(p => skus.Contains(p.Sku.ToUpper()))
            .ToListAsync(cancellationToken);

        if (products.Count != skus.Count)
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

            var product = products.First(p => p.Sku.Equals(item.Sku, StringComparison.OrdinalIgnoreCase));
            var inventory = product.Inventory;
            var available = inventory?.Quantity ?? 0;

            if (available < item.Quantity)
            {
                throw new ValidationException($"Insufficient stock for product {product.Sku}.");
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
        var paymentProvider = EntityDtoMapper.MapProviderDto(request.PaymentProvider);
        var status = paymentProvider == CaraDog.Db.Enums.PaymentProvider.Paypal
            ? DbOrderStatus.Paid
            : DbOrderStatus.PaymentPending;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Customer = customer,
            CreatedAt = DateTime.UtcNow,
            Status = status,
            PaymentProvider = paymentProvider,
            SubtotalNet = subtotalNet,
            TaxAmount = taxAmount,
            ShippingCost = shippingCost,
            TotalGross = totalGross,
            Items = orderItems
        };

        _dbContext.Customers.Add(customer);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var orderDto = order.ToDto();
        if (status == DbOrderStatus.Paid)
        {
            try
            {
                await _emailService.SendOrderConfirmationAsync(orderDto, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "HBH-ORD-004 Failed to send confirmation email for {OrderId}", order.Id);
            }
        }

        _logger.LogInformation("HBH-ORD-001 Order created {OrderId} {Status}", order.Id, status);

        return orderDto;
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Customer)
            .ThenInclude(c => c.City)
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

        if (string.IsNullOrWhiteSpace(request.Customer.Street))
        {
            throw new ValidationException("Shipping street is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Customer.HouseNumber))
        {
            throw new ValidationException("Shipping house number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Customer.CityName))
        {
            throw new ValidationException("Shipping city is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Customer.PostalCode))
        {
            throw new ValidationException("Shipping postal code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Customer.CountryCode))
        {
            throw new ValidationException("Shipping country code is required.");
        }
    }

    public async Task<CartInfoDto> GetCartInfoAsync(CartInfoRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException("Cart must contain at least one item.");
        }

        var skus = request.Items
            .Select(item => item.Sku.Trim())
            .Where(sku => !string.IsNullOrWhiteSpace(sku))
            .Select(sku => sku.ToUpperInvariant())
            .Distinct()
            .ToList();

        var products = await _dbContext.Products
            .Include(p => p.Inventory)
            .Where(p => skus.Contains(p.Sku.ToUpper()))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (products.Count != skus.Count)
        {
            throw new ValidationException("One or more products do not exist.");
        }

        var taxCalculator = _taxResolver.Resolve("AT");
        var lines = new List<CartLineDto>();
        decimal subtotalNet = 0;
        decimal taxAmount = 0;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ValidationException("Item quantity must be greater than zero.");
            }

            var product = products.First(p => p.Sku.Equals(item.Sku, StringComparison.OrdinalIgnoreCase));
            var available = product.Inventory?.Quantity ?? 0;

            if (available < item.Quantity)
            {
                throw new ValidationException($"Insufficient stock for product {product.Sku}.");
            }

            var lineNet = Math.Round(product.NetPrice * item.Quantity, 2, MidpointRounding.AwayFromZero);
            var lineTax = taxCalculator.CalculateTax(lineNet);
            subtotalNet += lineNet;
            taxAmount += lineTax;

            lines.Add(new CartLineDto(
                product.Sku,
                product.Name,
                item.Quantity,
                product.NetPrice,
                lineNet,
                lineTax,
                lineNet + lineTax));
        }

        var shippingCost = subtotalNet >= FreeShippingThreshold ? 0 : ShippingFlatRate;
        var totalGross = subtotalNet + taxAmount + shippingCost;

        return new CartInfoDto(lines, subtotalNet, taxAmount, shippingCost, totalGross);
    }

    private async Task<City> GetOrCreateCityAsync(CustomerCreateRequest request, CancellationToken cancellationToken)
    {
        var name = request.CityName.Trim();
        var postalCode = request.PostalCode.Trim();
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();

        var city = await _dbContext.Cities
            .FirstOrDefaultAsync(
                c => c.Name == name && c.PostalCode == postalCode && c.CountryCode == countryCode,
                cancellationToken);

        if (city is not null)
        {
            return city;
        }

        city = new City
        {
            Id = Guid.NewGuid(),
            Name = name,
            PostalCode = postalCode,
            CountryCode = countryCode
        };

        _dbContext.Cities.Add(city);
        return city;
    }
}
