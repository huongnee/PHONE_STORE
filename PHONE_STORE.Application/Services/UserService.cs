using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo) => _repo = repo;

    public Task<List<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<UserDto?> GetUserByIdAsync(long id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);
}
