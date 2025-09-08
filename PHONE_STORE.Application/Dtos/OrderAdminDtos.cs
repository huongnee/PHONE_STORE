using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos;

public record OrderListItemDto(
    long Id, string Code, string Status, string? CustomerEmail,
    decimal GrandTotal, DateTime PlacedAt);

public record OrderItemLineDto(
    long VariantId, string? ProductName, string? Sku,
    int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderDetailDto(
    long Id, string Code, string Status, long? CustomerId, long? ShippingAddressId,
    decimal Subtotal, decimal DiscountTotal, decimal TaxTotal, decimal ShippingFee, decimal GrandTotal,
    string? Note, string Currency, DateTime PlacedAt, DateTime? UpdatedAt,
    List<OrderItemLineDto> Items);
