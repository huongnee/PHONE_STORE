// PHONE_STORE.Infrastructure/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PhoneDbContext _db;
    public UserRepository(PhoneDbContext db) => _db = db;

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.UserAccounts
            .OrderByDescending(x => x.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Phone = u.Phone,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            }).ToListAsync(ct);

    public async Task<UserDto?> GetByIdAsync(long id, CancellationToken ct = default)
        => await _db.UserAccounts
            .Where(x => x.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Phone = u.Phone,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            }).FirstOrDefaultAsync(ct);

    public async Task<UserDto?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var uname = (username ?? "").Trim().ToLower();
        return await _db.UserAccounts
            .Where(x => x.Email == uname || x.Phone == username)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Phone = u.Phone,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<AuthUserDto?> GetAuthUserAsync(string username, CancellationToken ct = default)
    {
        var uname = (username ?? "").Trim().ToLower();
        var u = await _db.UserAccounts.FirstOrDefaultAsync(x => x.Email == uname || x.Phone == username, ct);
        if (u == null) return null;

        return new AuthUserDto
        {
            Id = u.Id,
            Email = u.Email,
            Phone = u.Phone,
            Status = u.Status,
            PasswordHash = u.PasswordHash,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task<string[]> GetRolesAsync(long userId, CancellationToken ct = default)
        => await (from ur in _db.UserRoles
                  join r in _db.Roles on ur.RoleId equals r.Id
                  where ur.UserId == userId
                  select r.Code).ToArrayAsync(ct);

    // ===== NEW: đăng ký =====
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var em = (email ?? "").Trim().ToLower();
        var count = await _db.UserAccounts.CountAsync(x => x.Email == em, ct);
        return count > 0;
    }

    public async Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default)
    {
        var ph = (phone ?? "").Trim();
        var count = await _db.UserAccounts.CountAsync(x => x.Phone == ph, ct);
        return count > 0;
    }


    public async Task<long> CreateAsync(string email, string passwordHash, string? phone, CancellationToken ct = default)
    {
        var u = new UserAccount
        {
            Email = email.Trim().ToLower(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            PasswordHash = passwordHash,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };
        _db.UserAccounts.Add(u);
        await _db.SaveChangesAsync(ct);
        return u.Id;
    }

    public async Task AddRoleAsync(long userId, string roleCode, CancellationToken ct = default)
    {
        var code = (roleCode ?? "").Trim().ToUpperInvariant();

        var roleId = await _db.Roles
            .Where(r => r.Code.ToUpper() == code)
            .Select(r => r.Id)
            .SingleOrDefaultAsync(ct);

        if (roleId == 0)
            throw new InvalidOperationException($"Role code '{code}' not found in ROLES.");

        _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AuthUserDto?> GetAuthUserByIdAsync(long userId, CancellationToken ct = default)
    {
        var u = await _db.UserAccounts.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (u == null) return null;
        return new AuthUserDto
        {
            Id = u.Id,
            Email = u.Email,
            Phone = u.Phone,
            Status = u.Status,
            PasswordHash = u.PasswordHash,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task UpdatePasswordHashAsync(long userId, string newHash, CancellationToken ct = default)
    {
        var u = await _db.UserAccounts.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (u == null) return;
        u.PasswordHash = newHash;
        await _db.SaveChangesAsync(ct);
    }


}
