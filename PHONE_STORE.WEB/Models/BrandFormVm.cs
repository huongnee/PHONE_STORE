public class BrandFormVm
{
    public long? Id { get; set; }
    public string Name { get; set; } = "";
    public string? Slug { get; set; }          // để trống sẽ tự sinh
    public bool IsActive { get; set; } = true;
}
