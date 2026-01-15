using CaraDog.DTO.Addresses;

namespace CaraDog.Core.Abstractions.Services;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AddressDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AddressDto> CreateAsync(AddressCreateRequest request, CancellationToken cancellationToken = default);
    Task<AddressDto> UpdateAsync(Guid id, AddressUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
