using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PHONE_STORE.WEB.Models;

public class ProductFormVm
{
    public long? Id { get; set; }

    [Required] public long BrandId { get; set; }
    public long? DefaultCategoryId { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = default!;

    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? SpecJson { get; set; }
    public bool IsActive { get; set; }

    public List<SelectListItem> BrandOptions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
}
