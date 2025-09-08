using System.Collections.Generic;
using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.WEB.Models
{
    public class CheckoutPageVm
    {
        public List<AddressDto> Addresses { get; set; } = new();
        public long? SelectedAddressId { get; set; }
        public CheckoutPreviewResult Preview { get; set; } =
            new CheckoutPreviewResult(new List<CheckoutPreviewItem>(), 0m, 0m, 0m, 0m, 0m);
    }
}
