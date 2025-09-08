using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class CartItem
    {
        public long CartId { get; set; }
        public long VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }     // DB cho phép NULL
        public string Currency { get; set; } = "VND";
        public DateTime AddedAt { get; set; }
        public Cart Cart { get; set; } = default!;
    }
}
