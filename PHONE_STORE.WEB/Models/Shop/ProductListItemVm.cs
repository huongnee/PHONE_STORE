namespace PHONE_STORE.WEB.Models.Shop;

public record ProductListItemVm(long Id, string Name, string Slug, string? ImageUrl, decimal MinPrice);
