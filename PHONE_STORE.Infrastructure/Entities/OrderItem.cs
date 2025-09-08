using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long VariantId { get; set; }
        public string? ProductName { get; set; }    // PRODUCT_NAME
        public string? Sku { get; set; }            // SKU
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "VND";
        public decimal TaxAmount { get; set; }      // TAX_AMOUNT
        public decimal DiscountAmount { get; set; } // DISCOUNT_AMOUNT
        public Order Order { get; set; } = default!;
    }
}
