using System.ComponentModel.DataAnnotations;

namespace PHONE_STORE.WEB.Models;

public class PriceFormVm
{
    public long? Id { get; set; }
    [Required] public long VariantId { get; set; }
    [Required] public decimal ListPrice { get; set; }
    public decimal? SalePrice { get; set; }
    [Required, StringLength(10)] public string Currency { get; set; } = "VND";
    [Required] public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndsAt { get; set; }
}
