using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class Order
    {
        public long Id { get; set; }
        public string Code { get; set; } = "";
        public long? CustomerId { get; set; }
        public long? ShippingAddressId { get; set; }
        public string Status { get; set; } = "PENDING";
        public string Currency { get; set; } = "VND";
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Note { get; set; }
        public DateTime PlacedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ✨ Ghi rõ kiểu để tránh suy luận sai
        public ICollection<PHONE_STORE.Infrastructure.Entities.OrderItem> Items { get; set; }
            = new List<PHONE_STORE.Infrastructure.Entities.OrderItem>();
    }

}
