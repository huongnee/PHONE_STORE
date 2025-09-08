using PHONE_STORE.Infrastructure.Entities;


namespace PHONE_STORE.Infrastructure.Entities
{
    public class Cart
    {
        public long Id { get; set; }
        public long? CustomerId { get; set; }
        public string? SessionId { get; set; }      // map -> SESSION_TOKEN
        public string Currency { get; set; } = "VND";
        public string Status { get; set; } = "ACTIVE";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}