using System.Text.Json.Serialization;
using CaraDog.Api.Middleware;
using CaraDog.Core.Abstractions.Commands;
using CaraDog.Core.Abstractions.Email;
using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Abstractions.Tax;
using CaraDog.Core.Commands.Addresses;
using CaraDog.Core.Commands.Categories;
using CaraDog.Core.Commands.Customers;
using CaraDog.Core.Commands.Inventory;
using CaraDog.Core.Commands.Orders;
using CaraDog.Core.Commands.Products;
using CaraDog.Core.Email;
using CaraDog.Core.Services;
using CaraDog.Core.Tax;
using CaraDog.Db;
using CaraDog.DTO.Addresses;
using CaraDog.DTO.Categories;
using CaraDog.DTO.Customers;
using CaraDog.DTO.Inventory;
using CaraDog.DTO.Orders;
using CaraDog.DTO.Products;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("CaraDogDb");
builder.Services.AddDbContext<CaraDogDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// SERVICES
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, LoggingEmailService>();

builder.Services.AddSingleton<ITaxCalculator, TaxCalculatorAT>();
builder.Services.AddSingleton<ITaxCalculator, TaxCalculatorDE>();
builder.Services.AddSingleton<ITaxCalculatorResolver, TaxCalculatorResolver>();

//COMMANDS
builder.Services.AddScoped<ICommandHandler<GetProductsQuery, IReadOnlyList<ProductDto>>, GetProductsHandler>();
builder.Services.AddScoped<ICommandHandler<GetProductByIdQuery, ProductDto>, GetProductByIdHandler>();
builder.Services.AddScoped<ICommandHandler<CreateProductCommand, ProductDto>, CreateProductHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProductCommand, ProductDto>, UpdateProductHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteProductCommand, bool>, DeleteProductHandler>();

builder.Services.AddScoped<ICommandHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>, GetCategoriesHandler>();
builder.Services.AddScoped<ICommandHandler<GetCategoryByIdQuery, CategoryDto>, GetCategoryByIdHandler>();
builder.Services.AddScoped<ICommandHandler<CreateCategoryCommand, CategoryDto>, CreateCategoryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateCategoryCommand, CategoryDto>, UpdateCategoryHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteCategoryCommand, bool>, DeleteCategoryHandler>();

builder.Services.AddScoped<ICommandHandler<GetCustomersQuery, IReadOnlyList<CustomerDto>>, GetCustomersHandler>();
builder.Services.AddScoped<ICommandHandler<GetCustomerByIdQuery, CustomerDto>, GetCustomerByIdHandler>();
builder.Services.AddScoped<ICommandHandler<CreateCustomerCommand, CustomerDto>, CreateCustomerHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateCustomerCommand, CustomerDto>, UpdateCustomerHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteCustomerCommand, bool>, DeleteCustomerHandler>();

builder.Services.AddScoped<ICommandHandler<GetAddressesQuery, IReadOnlyList<AddressDto>>, GetAddressesHandler>();
builder.Services.AddScoped<ICommandHandler<GetAddressByIdQuery, AddressDto>, GetAddressByIdHandler>();
builder.Services.AddScoped<ICommandHandler<CreateAddressCommand, AddressDto>, CreateAddressHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateAddressCommand, AddressDto>, UpdateAddressHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteAddressCommand, bool>, DeleteAddressHandler>();

builder.Services.AddScoped<ICommandHandler<GetInventoriesQuery, IReadOnlyList<InventoryDto>>, GetInventoriesHandler>();
builder.Services.AddScoped<ICommandHandler<GetInventoryByIdQuery, InventoryDto>, GetInventoryByIdHandler>();
builder.Services.AddScoped<ICommandHandler<GetInventoryByProductIdQuery, InventoryDto>, GetInventoryByProductIdHandler>();
builder.Services.AddScoped<ICommandHandler<CreateInventoryCommand, InventoryDto>, CreateInventoryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateInventoryCommand, InventoryDto>, UpdateInventoryHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteInventoryCommand, bool>, DeleteInventoryHandler>();

builder.Services.AddScoped<ICommandHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>, GetOrdersHandler>();
builder.Services.AddScoped<ICommandHandler<GetOrderByIdQuery, OrderDto>, GetOrderByIdHandler>();
builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, OrderDto>, CreateOrderHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateOrderStatusCommand, OrderDto>, UpdateOrderStatusHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteOrderCommand, bool>, DeleteOrderHandler>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ProblemDetailsMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
