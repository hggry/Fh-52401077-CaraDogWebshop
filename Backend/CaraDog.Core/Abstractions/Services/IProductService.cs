using CaraDog.DTO.Products;

namespace CaraDog.Core.Abstractions.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(ProductCreateRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, ProductUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
