using PHONE_STORE.Application.Dtos;

public interface IBrandRepository
{
    Task<PagedResult<BrandDto>> SearchAsync(string? q, int page, int pageSize, CancellationToken ct);
    Task<BrandDto?> GetAsync(long id, CancellationToken ct);
    Task<long> CreateAsync(BrandCreateDto dto, CancellationToken ct);
    Task<bool> UpdateAsync(long id, BrandUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(long id, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, long? exceptId, CancellationToken ct);
}
