namespace PHONE_STORE.Infrastructure.Entities;
public class ProductVariant
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string Sku { get; set; } = default!;
    public string? Color { get; set; }
    public int? StorageGb { get; set; }
    public string? Barcode { get; set; }
    public decimal? WeightGram { get; set; }
    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = default!;
    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    // PHONE_STORE.Infrastructure/Entities/ProductVariant.cs (thêm)
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<DeviceUnit> DeviceUnits { get; set; } = new List<DeviceUnit>();

}