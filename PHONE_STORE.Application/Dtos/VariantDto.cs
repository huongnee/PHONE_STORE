using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos
{
    public record VariantDto(
    long Id, long ProductId, string Sku, string? Color, int? StorageGb,
    string? Barcode, decimal? WeightGram, bool IsActive);

    public record VariantCreateDto(
        long ProductId, string Sku, string? Color, int? StorageGb,
        string? Barcode, decimal? WeightGram, bool IsActive);

    public record VariantUpdateDto(
        string Sku, string? Color, int? StorageGb, string? Barcode,
        decimal? WeightGram, bool IsActive);
}
