using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PHONE_STORE.WEB.Models;

public class CategoryFormVm
{
    public long? Id { get; set; }

    [Display(Name = "Danh mục cha")]
    public long? ParentId { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(150)]
    public string? Slug { get; set; } // để trống -> auto

    [Display(Name = "Thứ tự")]
    public int SortOrder { get; set; } = 0;

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;

    // for dropdown
    public List<SelectListItem> ParentOptions { get; set; } = new();
}
