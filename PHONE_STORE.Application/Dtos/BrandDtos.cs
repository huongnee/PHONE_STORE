namespace PHONE_STORE.Application.Dtos;

public record BrandDto(long Id, string Name, string Slug, bool IsActive);
public record BrandCreateDto(string Name, string? Slug, bool IsActive);
public record BrandUpdateDto(string Name, string? Slug, bool IsActive);
