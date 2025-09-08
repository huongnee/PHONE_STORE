using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly PhoneDbContext _db;
    public AddressRepository(PhoneDbContext db) => _db = db;

    public async Task<List<AddressDto>> GetByCustomerAsync(long customerId, CancellationToken ct = default)
        => await _db.Addresses.AsNoTracking().Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)
            .Select(a => new AddressDto(a.Id, a.CustomerId, a.Label, a.Recipient, a.Phone, a.Line1,
                                        a.Ward, a.District, a.Province, a.PostalCode,
                                        a.AddressType, a.IsDefault, a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

    public async Task<AddressDto?> GetAsync(long id, CancellationToken ct = default)
        => await _db.Addresses.AsNoTracking().Where(a => a.Id == id)
            .Select(a => new AddressDto(a.Id, a.CustomerId, a.Label, a.Recipient, a.Phone, a.Line1,
                                        a.Ward, a.District, a.Province, a.PostalCode,
                                        a.AddressType, a.IsDefault, a.CreatedAt, a.UpdatedAt))
            .FirstOrDefaultAsync(ct);

    public async Task<long> CreateAsync(long customerId, AddressUpsertDto dto, CancellationToken ct = default)
    {
        var e = new Address
        {
            CustomerId = customerId,
            Label = dto.Label?.Trim(),
            Recipient = dto.Recipient.Trim(),
            Phone = dto.Phone.Trim(),
            Line1 = dto.Line1.Trim(),
            Ward = dto.Ward?.Trim(),
            District = dto.District?.Trim(),
            Province = dto.Province?.Trim(),
            PostalCode = dto.PostalCode?.Trim(),
            AddressType = (dto.AddressType ?? "SHIPPING").ToUpperInvariant(),
            IsDefault = dto.IsDefault,
            CreatedAt = DateTime.UtcNow
        };
        _db.Addresses.Add(e);
        await _db.SaveChangesAsync(ct);

        if (e.IsDefault)
            await SetDefaultAsync(e.Id, e.AddressType, ct);

        return e.Id;
    }

    public async Task<bool> UpdateAsync(long id, AddressUpsertDto dto, CancellationToken ct = default)
    {
        var e = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (e == null) return false;

        e.Label = dto.Label?.Trim();
        e.Recipient = dto.Recipient.Trim();
        e.Phone = dto.Phone.Trim();
        e.Line1 = dto.Line1.Trim();
        e.Ward = dto.Ward?.Trim();
        e.District = dto.District?.Trim();
        e.Province = dto.Province?.Trim();
        e.PostalCode = dto.PostalCode?.Trim();
        e.AddressType = (dto.AddressType ?? "SHIPPING").ToUpperInvariant();
        e.IsDefault = dto.IsDefault;
        e.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        if (e.IsDefault)
            await SetDefaultAsync(e.Id, e.AddressType, ct);

        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var e = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (e == null) return false;
        _db.Remove(e);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // đảm bảo mỗi (customer, type) chỉ có 1 default
    public async Task<bool> SetDefaultAsync(long id, string addressType, CancellationToken ct = default)
    {
        var e = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (e == null) return false;
        var type = (addressType ?? e.AddressType).ToUpperInvariant();

        await _db.Addresses
            .Where(a => a.CustomerId == e.CustomerId && a.AddressType == type && a.Id != e.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);

        e.IsDefault = true;
        e.AddressType = type;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}