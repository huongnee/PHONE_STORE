namespace PHONE_STORE.Infrastructure.Entities;
public class UserAccount
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PasswordHash { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
}
