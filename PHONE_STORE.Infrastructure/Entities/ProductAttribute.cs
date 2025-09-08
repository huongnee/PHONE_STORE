namespace PHONE_STORE.Infrastructure.Entities
{
    public class ProductAttribute
    {
        public long Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string DataType { get; set; } = "TEXT"; // INT/DECIMAL/BOOL/TEXT

        // Thêm dòng này để hết lỗi attr.Values
        public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
    }
}
