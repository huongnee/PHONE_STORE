using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(long id, CancellationToken ct = default);
}
