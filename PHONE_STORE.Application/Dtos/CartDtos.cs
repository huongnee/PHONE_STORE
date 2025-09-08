using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos;

public record CartItemUpsertDto(long VariantId, int Quantity);
public record CartItemDto(long Id, long VariantId, string Sku, string? Color, int? StorageGb,
                          decimal UnitPrice, int Quantity, decimal LineTotal);
public record CartDto(long Id, long? CustomerId, string? SessionId,
                      List<CartItemDto> Items, decimal Subtotal, string Currency = "VND");

//public record CheckoutPreviewRequest(long? AddressId);
//public record CheckoutPreviewResult(decimal Subtotal, decimal Vat, decimal ShippingFee, decimal GrandTotal);
//public record CheckoutSubmitRequest(long AddressId, string PaymentMethod);
//public record CheckoutSubmitResult(long OrderId, string Code, string Status, decimal GrandTotal);
