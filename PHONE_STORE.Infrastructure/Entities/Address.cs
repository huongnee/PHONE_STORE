using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class Address
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string? Label { get; set; }
        public string Recipient { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Line1 { get; set; } = null!;
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        // 'SHIPPING' | 'BILLING'
        public string AddressType { get; set; } = "SHIPPING";
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
