namespace PHONE_STORE.Application.Dtos;

public sealed record PriceDto(
    long Id,
    long VariantId,
    decimal ListPrice,
    decimal? SalePrice,
    string Currency,
    DateTime StartsAt,
    DateTime? EndsAt
);

public sealed record PriceUpsertDto(
    long VariantId,
    decimal ListPrice,
    decimal? SalePrice,
    string Currency,
    DateTime StartsAt,
    DateTime? EndsAt
);
