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

public class CustomerRepository : ICustomerRepository
{
    private readonly PhoneDbContext _db;
    public CustomerRepository(PhoneDbContext db) => _db = db;

    public async Task<List<CustomerDto>> SearchAsync(string? q, int top = 100, CancellationToken ct = default)
    {
        var s = (q ?? "").Trim().ToLower();
        var query = _db.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(s))
        {
            query = query.Where(c =>
                (c.Email ?? "").ToLower().Contains(s) ||
                (c.Phone ?? "").Contains(s) ||
                (c.FullName ?? "").ToLower().Contains(s));
        }
        top = Math.Clamp(top, 1, 500);
        return await query.OrderByDescending(c => c.CreatedAt).Take(top)
            .Select(c => new CustomerDto(c.Id, c.UserAccountId, c.Email, c.Phone, c.FullName, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<CustomerDto?> GetAsync(long id, CancellationToken ct = default)
        => await _db.Customers.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new CustomerDto(c.Id, c.UserAccountId, c.Email, c.Phone, c.FullName, c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(ct);

    public async Task<long> CreateAsync(CustomerUpsertDto dto, CancellationToken ct = default)
    {
        var e = new Customer
        {
            UserAccountId = dto.UserAccountId,
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim(),
            FullName = dto.FullName?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Customers.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> UpdateAsync(long id, CustomerUpsertDto dto, CancellationToken ct = default)
    {
        var e = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (e == null) return false;
        e.UserAccountId = dto.UserAccountId;
        e.Email = dto.Email?.Trim();
        e.Phone = dto.Phone?.Trim();
        e.FullName = dto.FullName?.Trim();
        e.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
