namespace PHONE_STORE.WEB.Models.Shop;

public record VariantVm(long Id, string Sku, string? Color, int? StorageGb, decimal Price);
public record ImageVm(long Id, string Url, string? Alt, int Sort);
public record ProductDetailVm(long Id, string Name, string Slug, string? Description,
                              List<VariantVm> Variants, List<ImageVm> Images);
