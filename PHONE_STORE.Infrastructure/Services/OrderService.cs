using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        
        private readonly PhoneDbContext _db;
        private readonly ICartRepository _carts;

        public OrderService(PhoneDbContext db, ICartRepository carts)
        {
            _db = db;
            _carts = carts;
        }

        // ===== PREVIEW =====
        public async Task<CheckoutPreviewResult> PreviewAsync(long userId, long addressId, string? sessionId = null, CancellationToken ct = default)
        {
            // map user -> customer
            var customerId = await _db.Customers.AsNoTracking()
                .Where(c => c.UserAccountId == userId)
                .Select(c => (long?)c.Id)
                .FirstOrDefaultAsync(ct);

            // Lưu ý: có thể để null (guest) nếu user chưa có customer row
            var cart = await _carts.GetOrCreateAsync(customerId, sessionId, ct);

            var items = cart.Items.Select(i => new CheckoutPreviewItem(
                i.VariantId,
                $"SKU-{i.VariantId}",
                null,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity
            )).ToList();

            var subtotal = items.Sum(x => x.LineTotal);
            decimal tax = 0m, ship = 0m, discount = 0m;
            var grand = subtotal + tax + ship - discount;

            return new CheckoutPreviewResult(items, subtotal, tax, ship, discount, grand);
        }

        // ===== SUBMIT =====
        public async Task<long?> SubmitAsync(long userId, long addressId, string? note, string? sessionId = null, CancellationToken ct = default)
        {
            // map user -> customer
            var customerId = await _db.Customers.AsNoTracking()
                .Where(c => c.UserAccountId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(ct);

            if (customerId == 0) return null;

            // lấy giỏ theo customerId
            var cart = await _carts.GetOrCreateAsync(customerId, sessionId, ct);
            if (cart.Items.Count == 0) return null;

            var subtotal = cart.Items.Sum(x => x.UnitPrice * x.Quantity);
            decimal tax = 0m, ship = 0m, discount = 0m;
            var grand = subtotal + tax + ship - discount;

            var order = new Order
            {
                Code = await GenerateCodeAsync(ct),
                CustomerId = customerId,
                ShippingAddressId = addressId,
                Status = "PENDING",
                Subtotal = subtotal,
                TaxTotal = tax,
                ShippingFee = ship,
                GrandTotal = grand,
                Note = note,
                PlacedAt = DateTime.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            foreach (var it in cart.Items)
            {
                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    VariantId = it.VariantId,
                    ProductName = $"SKU-{it.VariantId}",
                    Sku = null,
                    UnitPrice = it.UnitPrice,
                    Quantity = it.Quantity,
                    TaxAmount = 0m,
                    DiscountAmount = 0m
                });
            }

            await _db.SaveChangesAsync(ct);

            // Clear giỏ hàng sau khi tạo order thành công (theo customerId)
            await _carts.ClearAsync(customerId, sessionId, ct);

            return order.Id;
        }


        private async Task<string> GenerateCodeAsync(CancellationToken ct)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var rnd = Random.Shared.Next(0, 999).ToString("000");
            var code = $"ORD{stamp}-{rnd}";
            var exists = await _db.Orders.AnyAsync(o => o.Code == code, ct);
            return exists ? $"{code}-{Guid.NewGuid().ToString("N")[..4]}" : code;
        }
    }
}
