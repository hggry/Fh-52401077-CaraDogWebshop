using System.Text.Json.Serialization;
using CaraDog.Api.Middleware;
using CaraDog.Core.Abstractions.Email;
using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Abstractions.Tax;
using CaraDog.Core.Email;
using CaraDog.Core.Services;
using CaraDog.Core.Tax;
using CaraDog.Db;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var logPath = context.Configuration["Logging:FilePath"] ?? "Logs/log-.txt";
    var logTable = context.Configuration["Logging:DatabaseTable"] ?? "LogEntries";
    var logConnection = context.Configuration.GetConnectionString("CaraDogDb")
        ?? "server=localhost;port=3306;database=caradog;user=app;password=apppw;";

    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
        .WriteTo.MySQL(logConnection, logTable);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:8081")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("CaraDogDb")
    ?? "server=localhost;port=3306;database=caradog;user=app;password=apppw;";
builder.Services.AddDbContext<CaraDogDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// SERVICES
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddSingleton<ITaxCalculator, TaxCalculatorAT>();
builder.Services.AddSingleton<ITaxCalculator, TaxCalculatorDE>();
builder.Services.AddSingleton<ITaxCalculatorResolver, TaxCalculatorResolver>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ProblemDetailsMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
