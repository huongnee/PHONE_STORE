using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class ProductPrice
    {
        public long Id { get; set; }
        public long VariantId { get; set; }
        public decimal ListPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public string Currency { get; set; } = "VND";
        public DateTime StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }

        public ProductVariant Variant { get; set; } = default!;

    }
}
