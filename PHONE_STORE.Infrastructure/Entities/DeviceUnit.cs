using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class DeviceUnit
    {
        public long Id { get; set; }
        public long VariantId { get; set; }
        public string Imei { get; set; } = null!;
        public string? SerialNo { get; set; }
        // IN_STOCK | SOLD | RETURNED
        public string Status { get; set; } = "IN_STOCK";
        public long? WarehouseId { get; set; } // bắt buộc khi IN_STOCK
        public DateTime ReceivedAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public DateTime? ReturnedAt { get; set; }

        public ProductVariant Variant { get; set; } = null!;
        public Warehouse? Warehouse { get; set; }
    }
}
