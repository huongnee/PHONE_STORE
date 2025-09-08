using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class Inventory
    {
        public long VariantId { get; set; }
        public long WarehouseId { get; set; }
        public int QtyOnHand { get; set; }
        public int QtyReserved { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ProductVariant Variant { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
    }
}
