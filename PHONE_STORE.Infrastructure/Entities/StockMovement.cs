using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class StockMovement
    {
        public long Id { get; set; }
        public long VariantId { get; set; }
        public long WarehouseId { get; set; }
        // IN | OUT | ADJUST
        public string MovementType { get; set; } = null!;
        // Lưu đúng “dấu” theo DB: IN >0; OUT <0; ADJUST != 0
        public int QtyDelta { get; set; }

        public string? RefType { get; set; }
        public long? RefId { get; set; }
        public string? RefCode { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? CreatedBy { get; set; }

        public ProductVariant Variant { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
    }
}
