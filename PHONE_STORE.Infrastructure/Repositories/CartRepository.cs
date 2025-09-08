using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;
using Microsoft.Extensions.Logging; // ✅

namespace PHONE_STORE.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly PhoneDbContext _db;
    private readonly ILogger<CartRepository> _log; // ✅

    public CartRepository(PhoneDbContext db, ILogger<CartRepository> log) // ✅
    {
        _db = db;
        _log = log;
    }

    private static decimal NowPriceForVariant(PhoneDbContext db, long variantId)
    {
        var now = DateTime.UtcNow;
        var p = db.ProductPrices.AsNoTracking()
            .Where(x => x.VariantId == variantId
                     && x.StartsAt <= now
                     && (x.EndsAt == null || x.EndsAt >= now))
            .OrderByDescending(x => x.StartsAt)
            .Select(x => x.SalePrice ?? x.ListPrice)
            .FirstOrDefault();

        return p;
    }

    private async Task<long> EnsureCartId(long? customerId, string? sessionId, CancellationToken ct)
    {
        _log.LogDebug("EnsureCartId IN: customerId={CustomerId}, sid={Sid}", customerId, sessionId);

        var q = _db.Carts.AsQueryable();
        Cart? cart = null;

        if (customerId.HasValue && customerId > 0)
        {
            var userCart = await q.FirstOrDefaultAsync(x => x.CustomerId == customerId, ct);
            Cart? guestCart = null;

            if (!string.IsNullOrWhiteSpace(sessionId))
                guestCart = await _db.Carts.FirstOrDefaultAsync(x => x.SessionId == sessionId, ct);

            if (userCart != null && guestCart != null && guestCart.Id != userCart.Id)
            {
                _log.LogInformation("EnsureCartId: merging sid={Sid} into customerId={CustomerId} (userCartId={U}, guestCartId={G})",
                    sessionId, customerId, userCart.Id, guestCart.Id);
                await MergeAsync(sessionId!, customerId.Value, ct);
                userCart = await q.FirstOrDefaultAsync(x => x.CustomerId == customerId, ct);
            }
            cart = userCart;
        }

        if (cart == null && !string.IsNullOrWhiteSpace(sessionId))
        {
            cart = await q.FirstOrDefaultAsync(x => x.SessionId == sessionId, ct);
            if (cart != null && customerId.HasValue && customerId > 0)
            {
                _log.LogInformation("EnsureCartId: claiming guest cart #{CartId} for customerId={CustomerId}, sid={Sid}",
                    cart.Id, customerId, sessionId);
                cart.CustomerId = customerId;
                cart.SessionId = null;
                cart.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        if (cart == null)
        {
            cart = new Infrastructure.Entities.Cart
            {
                CustomerId = customerId,
                SessionId = customerId.HasValue ? null : sessionId,
                Currency = "VND",
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct);

            _log.LogInformation("EnsureCartId: created new cart #{CartId} (customerId={CustomerId}, sid={Sid})",
                cart.Id, cart.CustomerId, cart.SessionId);
        }

        _log.LogDebug("EnsureCartId OUT: cartId={CartId}, customerId={CustomerId}, sid={Sid}",
            cart.Id, cart.CustomerId, cart.SessionId);
        return cart.Id;
    }

    private static CartDto ToDto(Infrastructure.Entities.Cart cart,
        List<(long VariantId, string Sku, string? Color, int? StorageGb, decimal UnitPrice, int Qty)> lines)
    {
        var items = lines
            .Select(l => new CartItemDto(
                Id: l.VariantId,
                VariantId: l.VariantId,
                Sku: l.Sku,
                Color: l.Color,
                StorageGb: l.StorageGb,
                UnitPrice: l.UnitPrice,
                Quantity: l.Qty,
                LineTotal: l.UnitPrice * l.Qty))
            .ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        return new CartDto(cart.Id, cart.CustomerId, cart.SessionId, items, subtotal, "VND");
    }

    public async Task<CartDto> GetOrCreateAsync(long? customerId, string? sessionId, CancellationToken ct)
    {
        var id = await EnsureCartId(customerId, sessionId, ct);
        var cart = await _db.Carts.AsNoTracking().FirstAsync(x => x.Id == id, ct);

        var rows = await
            (from ci in _db.CartItems.AsNoTracking().Where(x => x.CartId == id)
             join v in _db.ProductVariants.AsNoTracking() on ci.VariantId equals v.Id
             select new { ci, v })
            .ToListAsync(ct);

        var mapped = rows.Select(x =>
        {
            var unit = x.ci.UnitPrice ?? NowPriceForVariant(_db, x.ci.VariantId);
            return (x.v.Id, x.v.Sku, x.v.Color, x.v.StorageGb, unit, x.ci.Quantity);
        }).ToList();

        var dto = ToDto(cart, mapped);
        _log.LogDebug("GetOrCreate OUT: cartId={CartId}, items={Items}, dtoCustomerId={Cus}, dtoSid={Sid}",
            dto.Id, dto.Items.Count, dto.CustomerId, dto.SessionId);
        return dto;
    }

    public async Task<CartDto> AddOrUpdateItemAsync(long? customerId, string? sessionId, CartItemUpsertDto dto, CancellationToken ct)
    {
        var id = await EnsureCartId(customerId, sessionId, ct);

        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.CartId == id && x.VariantId == dto.VariantId, ct);
        if (item is null)
        {
            var price = NowPriceForVariant(_db, dto.VariantId);
            item = new Infrastructure.Entities.CartItem
            {
                CartId = id,
                VariantId = dto.VariantId,
                Quantity = Math.Max(1, dto.Quantity),
                UnitPrice = price,
                Currency = "VND",
                AddedAt = DateTime.UtcNow
            };
            _db.CartItems.Add(item);
            _log.LogInformation("AddItem: cartId={CartId}, variant={Variant}, qty={Qty}, unit={Unit}",
                id, dto.VariantId, item.Quantity, item.UnitPrice);
        }
        else
        {
            item.Quantity += dto.Quantity;
            if (item.Quantity < 1) item.Quantity = 1;
            _log.LogInformation("UpdateItemQty: cartId={CartId}, variant={Variant}, qty={Qty}",
                id, dto.VariantId, item.Quantity);
        }

        await _db.SaveChangesAsync(ct);
        return await GetOrCreateAsync(customerId, sessionId, ct);
    }

    public async Task<CartDto> UpdateQtyAsync(long itemId, int qty, long? customerId, string? sessionId, CancellationToken ct)
    {
        var id = await EnsureCartId(customerId, sessionId, ct);

        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.CartId == id && x.VariantId == itemId, ct);
        if (item == null)
        {
            _log.LogWarning("UpdateQty: item not found. cartId={CartId}, variant(itemId)={ItemId}", id, itemId);
            return await GetOrCreateAsync(customerId, sessionId, ct);
        }

        if (qty <= 0)
        {
            _db.CartItems.Remove(item);
            _log.LogInformation("RemoveItem: cartId={CartId}, variant={Variant}", id, itemId);
        }
        else
        {
            item.Quantity = qty;
            _log.LogInformation("SetQty: cartId={CartId}, variant={Variant}, qty={Qty}", id, itemId, qty);
        }

        await _db.SaveChangesAsync(ct);
        return await GetOrCreateAsync(customerId, sessionId, ct);
    }

    public async Task RemoveItemAsync(long itemId, long? customerId, string? sessionId, CancellationToken ct)
    {
        var id = await EnsureCartId(customerId, sessionId, ct);
        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.CartId == id && x.VariantId == itemId, ct);
        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync(ct);
            _log.LogInformation("RemoveItem: cartId={CartId}, variant={Variant}", id, itemId);
        }
        else
        {
            _log.LogWarning("RemoveItem: not found. cartId={CartId}, variant={Variant}", id, itemId);
        }
    }

    public async Task MergeAsync(string sessionId, long customerId, CancellationToken ct)
    {
        _log.LogInformation("Merge: sid={Sid} -> customerId={CustomerId}", sessionId, customerId);

        var guest = await _db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId, ct);

        var user = await _db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        if (guest == null)
        {
            _log.LogInformation("Merge: no guest cart found for sid={Sid}", sessionId);
            return;
        }

        if (user == null)
        {
            user = new Infrastructure.Entities.Cart
            {
                CustomerId = customerId,
                Currency = "VND",
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Carts.Add(user);
            _log.LogInformation("Merge: created user cart for customerId={CustomerId}", customerId);
        }

        foreach (var gi in guest.Items)
        {
            var ui = user.Items.FirstOrDefault(x => x.VariantId == gi.VariantId);
            if (ui == null)
            {
                user.Items.Add(new Infrastructure.Entities.CartItem
                {
                    VariantId = gi.VariantId,
                    Quantity = gi.Quantity,
                    UnitPrice = gi.UnitPrice,
                    Currency = gi.Currency,
                    AddedAt = DateTime.UtcNow
                });
                _log.LogDebug("Merge line: add variant={Variant}, qty={Qty}", gi.VariantId, gi.Quantity);
            }
            else
            {
                ui.Quantity += gi.Quantity;
                _log.LogDebug("Merge line: inc variant={Variant}, qty+={Qty}", gi.VariantId, gi.Quantity);
            }
        }

        _db.Carts.Remove(guest);
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Merge: done. guest cart removed.");
    }

    public async Task ClearAsync(long? customerId, string? sessionId, CancellationToken ct)
    {
        var id = await EnsureCartId(customerId, sessionId, ct);
        var items = _db.CartItems.Where(x => x.CartId == id);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Clear: cartId={CartId}", id);
    }
}
