namespace PHONE_STORE.Application.Dtos;

// item hiển thị trong preview
public record CheckoutPreviewItem(
    long VariantId, string Title, string? Sku,
    int Quantity, decimal UnitPrice, decimal LineTotal);

// KẾT QUẢ PREVIEW: 6 tham số (có DiscountTotal)
public record CheckoutPreviewResult(
    List<CheckoutPreviewItem> Items,
    decimal Subtotal,
    decimal TaxTotal,
    decimal ShippingFee,
    decimal DiscountTotal,
    decimal GrandTotal);

// request
public record CheckoutPreviewRequest(long AddressId);
public record CheckoutSubmitRequest(long AddressId, string? Note);
