using CaraDog.Core.Abstractions.Services;
using CaraDog.Core.Exceptions;
using CaraDog.Core.Mappers;
using CaraDog.Db;
using CaraDog.Db.Entities;
using CaraDog.DTO.Addresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CaraDog.Core.Services;

public sealed class AddressService : IAddressService
{
    private readonly CaraDogDbContext _dbContext;
    private readonly ILogger<AddressService> _logger;

    public AddressService(CaraDogDbContext dbContext, ILogger<AddressService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AddressDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var addresses = await _dbContext.Addresses
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return addresses.Select(address => address.ToDto()).ToList();
    }

    public async Task<AddressDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (address is null)
        {
            throw new NotFoundException($"Address {id} was not found.");
        }

        return address.ToDto();
    }

    public async Task<AddressDto> CreateAsync(AddressCreateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var address = new Address
        {
            Id = Guid.NewGuid(),
            Street = request.Street.Trim(),
            City = request.City.Trim(),
            PostalCode = request.PostalCode.Trim(),
            CountryCode = request.CountryCode.Trim().ToUpperInvariant(),
            State = request.State?.Trim()
        };

        _dbContext.Addresses.Add(address);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-ADR-001 Address created {AddressId}", address.Id);

        return address.ToDto();
    }

    public async Task<AddressDto> UpdateAsync(Guid id, AddressUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var address = await _dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (address is null)
        {
            throw new NotFoundException($"Address {id} was not found.");
        }

        address.Street = request.Street.Trim();
        address.City = request.City.Trim();
        address.PostalCode = request.PostalCode.Trim();
        address.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
        address.State = request.State?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-ADR-002 Address updated {AddressId}", address.Id);

        return address.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var address = await _dbContext.Addresses
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (address is null)
        {
            throw new NotFoundException($"Address {id} was not found.");
        }

        var inUse = await _dbContext.Orders
            .AnyAsync(o => o.ShippingAddressId == id, cancellationToken);

        if (inUse)
        {
            throw new ConflictException($"Address {id} is referenced by orders and cannot be deleted.");
        }

        _dbContext.Addresses.Remove(address);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HBH-ADR-003 Address deleted {AddressId}", id);
    }

    private static void ValidateRequest(AddressCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Street))
        {
            throw new ValidationException("Street is required.");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            throw new ValidationException("City is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PostalCode))
        {
            throw new ValidationException("Postal code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            throw new ValidationException("Country code is required.");
        }
    }

    private static void ValidateRequest(AddressUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Street))
        {
            throw new ValidationException("Street is required.");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            throw new ValidationException("City is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PostalCode))
        {
            throw new ValidationException("Postal code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode))
        {
            throw new ValidationException("Country code is required.");
        }
    }
}
