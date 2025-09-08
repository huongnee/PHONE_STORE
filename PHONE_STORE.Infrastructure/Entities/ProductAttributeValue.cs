using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class ProductAttributeValue
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long AttributeId { get; set; }
        public long? IntValue { get; set; }
        public decimal? DecValue { get; set; }
        public bool? BoolValue { get; set; } // map NUMBER(1,0)
        public string? TextValue { get; set; }

        public Product Product { get; set; } = default!;
        public ProductAttribute Attribute { get; set; } = default!;

    }
}
