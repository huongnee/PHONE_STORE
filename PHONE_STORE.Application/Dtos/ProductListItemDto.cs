namespace PHONE_STORE.Application.Dtos;

public sealed record ProductListItemDto(
    long Id,
    string Name,
    string Slug,
    string BrandName,
    string? CategoryName,
    bool IsActive,
    int ActiveVariantCount
);
