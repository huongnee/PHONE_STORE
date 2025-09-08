using System.ComponentModel.DataAnnotations;

namespace PHONE_STORE.WEB.Models;

public class VariantFormVm
{
    public long? Id { get; set; }
    [Required] public long ProductId { get; set; }
    [Required, StringLength(100)] public string Sku { get; set; } = default!;
    public string? Color { get; set; }
    public int? StorageGb { get; set; }
    public string? Barcode { get; set; }
    public decimal? WeightGram { get; set; }
    public bool IsActive { get; set; } = true;
}
