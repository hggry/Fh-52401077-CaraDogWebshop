using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Mappers;
using CaraDog.Db;
using CaraDog.Db.Entities;
using CaraDog.DTO.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly CaraDogDbContext _dbContext;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(CaraDogDbContext dbContext, ILogger<CustomerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return customers.Select(customer => customer.ToDto()).ToList();
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer {id} was not found.");
        }

        return customer.ToDto();
    }

    public async Task<CustomerDto> CreateAsync(CustomerCreateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-CUS-001 Customer created {CustomerId}", customer.Id);

        return customer.ToDto();
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, CustomerUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer {id} was not found.");
        }

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = request.Email.Trim();
        customer.Phone = request.Phone?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-CUS-002 Customer updated {CustomerId}", customer.Id);

        return customer.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer {id} was not found.");
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-CUS-003 Customer deleted {CustomerId}", id);
    }

    private static void ValidateRequest(CustomerCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ValidationException("Customer first name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ValidationException("Customer last name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Customer email is required.");
        }
    }

    private static void ValidateRequest(CustomerUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ValidationException("Customer first name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ValidationException("Customer last name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Customer email is required.");
        }
    }
}
