using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos
{
    
    public record ProductDto(
        long Id, long BrandId, long? DefaultCategoryId, string Name, string Slug,
        string? Description, string? SpecJson, bool IsActive);

    public record ProductCreateDto(
        long BrandId, long? DefaultCategoryId, string Name, string? Slug,
        string? Description, string? SpecJson, bool IsActive);

    public record ProductUpdateDto(
        long BrandId, long? DefaultCategoryId, string Name, string? Slug,
        string? Description, string? SpecJson, bool IsActive);
}
