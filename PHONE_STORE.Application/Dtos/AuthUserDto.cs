namespace PHONE_STORE.Application.Dtos;

// Dùng riêng cho Auth (có PasswordHash)
public class AuthUserDto
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}

