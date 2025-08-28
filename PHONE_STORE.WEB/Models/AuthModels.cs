namespace PHONE_STORE.WEB.Models;

public class RegisterRequest
{
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }       // NEW
}

public class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public DateTime AccessExpiresAt { get; set; }     // đổi tên cho khớp API (AccessExpiresAt)
    public string RefreshToken { get; set; } = "";
    public DateTime RefreshExpiresAt { get; set; }
    public long UserId { get; set; }
    public string? Email { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}
