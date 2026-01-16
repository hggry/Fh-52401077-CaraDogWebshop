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
            .Include(c => c.City)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return customers.Select(customer => customer.ToDto()).ToList();
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .Include(c => c.City)
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

        var city = await GetOrCreateCityAsync(request, cancellationToken);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone?.Trim(),
            Street = request.Street.Trim(),
            HouseNumber = request.HouseNumber.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            CityId = city.Id,
            City = city,
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
            .Include(c => c.City)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer {id} was not found.");
        }

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = request.Email.Trim();
        customer.Phone = request.Phone?.Trim();
        customer.Street = request.Street.Trim();
        customer.HouseNumber = request.HouseNumber.Trim();
        customer.AddressLine2 = request.AddressLine2?.Trim();

        var city = await GetOrCreateCityAsync(request, cancellationToken);
        customer.CityId = city.Id;
        customer.City = city;

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

        if (string.IsNullOrWhiteSpace(request.Street))
        {
            throw new ValidationException("Customer street is required.");
        }

        if (string.IsNullOrWhiteSpace(request.HouseNumber))
        {
            throw new ValidationException("Customer house number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CityName))
        {
            throw new ValidationException("Customer city is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PostalCode))
        {
            throw new ValidationException("Customer postal code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            throw new ValidationException("Customer country code is required.");
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

        if (string.IsNullOrWhiteSpace(request.Street))
        {
            throw new ValidationException("Customer street is required.");
        }

        if (string.IsNullOrWhiteSpace(request.HouseNumber))
        {
            throw new ValidationException("Customer house number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CityName))
        {
            throw new ValidationException("Customer city is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PostalCode))
        {
            throw new ValidationException("Customer postal code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            throw new ValidationException("Customer country code is required.");
        }
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

    private async Task<City> GetOrCreateCityAsync(CustomerUpdateRequest request, CancellationToken cancellationToken)
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
