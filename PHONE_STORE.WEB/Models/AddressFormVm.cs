using System.ComponentModel.DataAnnotations;

namespace PHONE_STORE.WEB.Models
{
    public class AddressFormVm
    {
        public long? Id { get; set; }
        public long CustomerId { get; set; }
        [StringLength(60)] public string? Label { get; set; }
        [Required, StringLength(150)] public string Recipient { get; set; } = null!;
        [Required, StringLength(20)] public string Phone { get; set; } = null!;
        [Required, StringLength(200)] public string Line1 { get; set; } = null!;
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        [Required] public string AddressType { get; set; } = "SHIPPING";
        public bool IsDefault { get; set; }
    }
}
