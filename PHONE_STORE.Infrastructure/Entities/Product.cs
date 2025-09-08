namespace PHONE_STORE.Infrastructure.Entities;
public class Product
{
    public long Id { get; set; }
    public long BrandId { get; set; }
    public long? DefaultCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public string? SpecJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Brand? Brand { get; set; }
    public Category? DefaultCategory { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();

}