using System.ComponentModel.DataAnnotations;

namespace PHONE_STORE.WEB.Models;

public class ImageFormVm
{
    public long? Id { get; set; }
    public long? ProductId { get; set; }
    public long? VariantId { get; set; }

    [Required] public string ImageUrl { get; set; } = default!;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
