using System.ComponentModel.DataAnnotations;

namespace PHONE_STORE.WEB.Models;

public class WarehouseFormVm
{
    public long? Id { get; set; }

    [Required, StringLength(32)]
    public string Code { get; set; } = null!;

    [Required, StringLength(120)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? AddressLine { get; set; }
    [StringLength(120)]
    public string? District { get; set; }
    [StringLength(120)]
    public string? Province { get; set; }

    public bool IsActive { get; set; } = true;
}