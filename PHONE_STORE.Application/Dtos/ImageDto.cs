using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos
{
    public record ImageDto(
    long Id, long? ProductId, long? VariantId, string ImageUrl,
    string? AltText, bool IsPrimary, int SortOrder);

    public record ImageCreateDto(
        long? ProductId, long? VariantId, string ImageUrl,
        string? AltText, bool IsPrimary, int SortOrder);
}
