using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PHONE_STORE.WEB.Models;

public class StockMoveVm
{
    [Required] public long VariantId { get; set; }
    [Required] public long WarehouseId { get; set; }
    [Required] public string MovementType { get; set; } = "IN"; // IN | OUT | ADJUST
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; }
    public string? RefType { get; set; }
    public long? RefId { get; set; }
    public string? RefCode { get; set; }
    public string? Note { get; set; }
    public string? ImeiCsv { get; set; } // nhập nhiều IMEI, phân tách bởi xuống dòng hoặc dấu phẩy

    public List<SelectListItem> WarehouseOptions { get; set; } = new();
}
