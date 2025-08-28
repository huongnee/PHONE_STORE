using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserDto>> GetAllAsync(CancellationToken ct = default);
        Task<UserDto?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<UserDto?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<AuthUserDto?> GetAuthUserAsync(string username, CancellationToken ct = default);
        Task<string[]> GetRolesAsync(long userId, CancellationToken ct = default);

        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task<bool> PhoneExistsAsync(string phone, CancellationToken ct = default);
        Task<long> CreateAsync(string email, string passwordHash, string? phone, CancellationToken ct = default);
        Task AddRoleAsync(long userId, string roleCode, CancellationToken ct = default);

        // NEW
        Task<AuthUserDto?> GetAuthUserByIdAsync(long userId, CancellationToken ct = default);
        Task UpdatePasswordHashAsync(long userId, string newHash, CancellationToken ct = default);
    }
}
