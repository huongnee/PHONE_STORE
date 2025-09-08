using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Infrastructure.Entities
{
    public class Customer
    {
        public long Id { get; set; }
        public long? UserAccountId { get; set; }       
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserAccount? UserAccount { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
