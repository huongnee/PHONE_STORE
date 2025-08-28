namespace PHONE_STORE.Infrastructure.Entities;

public class Brand
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
