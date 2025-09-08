using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class ProductImage
    {
        public long Id { get; set; }
        public long? ProductId { get; set; }
        public long? VariantId { get; set; }
        public string ImageUrl { get; set; } = default!;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public Product? Product { get; set; }

        public ProductVariant? Variant { get; set; }

    }
}
