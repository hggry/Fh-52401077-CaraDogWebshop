using CaraDog.Core.Abstractions.Email;
using CaraDog.Core.Abstractions.Tax;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Services;
using CaraDog.Core.Tax;
using CaraDog.Db.Entities;
using CaraDog.DTO.Addresses;
using CaraDog.DTO.Customers;
using CaraDog.DTO.Enums;
using CaraDog.DTO.Inventory;
using CaraDog.DTO.Orders;
using CaraDog.DTO.Products;
using CaraDog.Integrationtests.TestUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CaraDog.Integrationtests;

[TestClass]
public sealed class OrderServiceTests
{
    [TestMethod]
    public async Task CreateOrder_CalculatesTotalsAndUpdatesStock()
    {
        var dbContext = DbContextFactory.Create(nameof(CreateOrder_CalculatesTotalsAndUpdatesStock));

        var category = new Category { Id = Guid.NewGuid(), Name = "Food" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Kibble",
            Sku = "KB-1",
            NetPrice = 10m,
            CategoryId = category.Id,
            Category = category,
            IsSoldOut = false
        };
        var inventory = new Inventory { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Quantity = 2 };

        dbContext.Categories.Add(category);
        dbContext.Products.Add(product);
        dbContext.Inventories.Add(inventory);
        await dbContext.SaveChangesAsync();

        ITaxCalculator[] calculators = { new TaxCalculatorAT(), new TaxCalculatorDE() };
        var taxResolver = new TaxCalculatorResolver(calculators);
        IEmailService emailService = new Core.Email.LoggingEmailService(NullLogger<Core.Email.LoggingEmailService>.Instance);
        var service = new OrderService(
            dbContext,
            taxResolver,
            emailService,
            NullLogger<OrderService>.Instance);

        var request = new OrderCreateRequest(
            new CustomerCreateRequest("Ada", "Lovelace", "ada@caradog.local", "123"),
            new AddressCreateRequest("Street 1", "Vienna", "1010", "AT", null),
            new List<OrderItemCreateRequest> { new(product.Id, 2) },
            PaymentProvider.Paypal);

        var order = await service.CreateAsync(request);

        Assert.AreEqual(20m, order.SubtotalNet);
        Assert.AreEqual(4m, order.TaxAmount);
        Assert.AreEqual(7.99m, order.ShippingCost);
        Assert.AreEqual(31.99m, order.TotalGross);

        var refreshedInventory = await dbContext.Inventories.FindAsync(inventory.Id);
        Assert.IsNotNull(refreshedInventory);
        Assert.AreEqual(0, refreshedInventory.Quantity);

        var refreshedProduct = await dbContext.Products.FindAsync(product.Id);
        Assert.IsNotNull(refreshedProduct);
        Assert.IsTrue(refreshedProduct.IsSoldOut);
    }

    [TestMethod]
    public async Task CreateOrder_WithInsufficientStock_Throws()
    {
        var dbContext = DbContextFactory.Create(nameof(CreateOrder_WithInsufficientStock_Throws));

        var category = new Category { Id = Guid.NewGuid(), Name = "Food" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Kibble",
            Sku = "KB-2",
            NetPrice = 10m,
            CategoryId = category.Id,
            Category = category
        };
        var inventory = new Inventory { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Quantity = 1 };

        dbContext.Categories.Add(category);
        dbContext.Products.Add(product);
        dbContext.Inventories.Add(inventory);
        await dbContext.SaveChangesAsync();

        ITaxCalculator[] calculators = { new TaxCalculatorAT(), new TaxCalculatorDE() };
        var taxResolver = new TaxCalculatorResolver(calculators);
        IEmailService emailService = new Core.Email.LoggingEmailService(NullLogger<Core.Email.LoggingEmailService>.Instance);
        var service = new OrderService(
            dbContext,
            taxResolver,
            emailService,
            NullLogger<OrderService>.Instance);

        var request = new OrderCreateRequest(
            new CustomerCreateRequest("Ada", "Lovelace", "ada@caradog.local", null),
            new AddressCreateRequest("Street 1", "Vienna", "1010", "AT", null),
            new List<OrderItemCreateRequest> { new(product.Id, 2) },
            PaymentProvider.Paypal);

        await Assert.ThrowsExceptionAsync<ValidationException>(() => service.CreateAsync(request));
    }
}
