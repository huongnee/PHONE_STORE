namespace PHONE_STORE.Infrastructure.Entities;
//Đại diện cho bảng ROLES
public class Role
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}
