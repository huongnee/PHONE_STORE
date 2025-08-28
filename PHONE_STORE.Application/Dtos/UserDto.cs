namespace PHONE_STORE.Application.Dtos;

public class UserDto
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
}
