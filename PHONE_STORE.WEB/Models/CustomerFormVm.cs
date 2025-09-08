using System.ComponentModel.DataAnnotations;

namespace PHONE_STORE.WEB.Models
{
    public class CustomerFormVm
    {
        public long? Id { get; set; }
        public long? UserAccountId { get; set; }
        [EmailAddress] public string? Email { get; set; }
        [StringLength(20)] public string? Phone { get; set; }
        [StringLength(150)] public string? FullName { get; set; }
    }
}
