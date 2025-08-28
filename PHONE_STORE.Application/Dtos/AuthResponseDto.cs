namespace PHONE_STORE.Application.Dtos;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = "";
    public DateTime AccessExpiresAt { get; set; }

    public string RefreshToken { get; set; } = "";
    public DateTime RefreshExpiresAt { get; set; }

    public long UserId { get; set; }
    public string? Email { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}
