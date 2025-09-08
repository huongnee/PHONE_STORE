using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.WEB.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly HttpClient _api;
    public CheckoutController(IHttpClientFactory f) => _api = f.CreateClient("api");

    public class CheckoutPageVm
    {
        public CheckoutPreviewResult Preview { get; set; } = default!;
        public List<AddressDto> Addresses { get; set; } = new();
        public long? SelectedAddressId { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var addrs = await _api.GetFromJsonAsync<List<AddressDto>>("/api/shop/me/addresses") ?? new();
        var selected = addrs.FirstOrDefault(a => a.IsDefault)?.Id
                       ?? addrs.FirstOrDefault()?.Id
                       ?? 0;

        var previewRes = await _api.PostAsJsonAsync("/api/shop/checkout/preview",
            new CheckoutPreviewRequest(selected));
        var preview = await previewRes.Content.ReadFromJsonAsync<CheckoutPreviewResult>()
                      ?? new(new List<CheckoutPreviewItem>(), 0, 0, 0, 0, 0);

        return View(new CheckoutPageVm { Preview = preview, Addresses = addrs, SelectedAddressId = selected });
    }


    public record SubmitOrderResponse(long Id);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(long addressId, string paymentMethod = "COD")
    {
        // API hiện nhận AddressId + Note (PaymentMethod nếu có thì để sau)
        var res = await _api.PostAsJsonAsync("/api/shop/checkout/submit",
            new CheckoutSubmitRequest(addressId, null));

        if (!res.IsSuccessStatusCode)
        {
            TempData["Err"] = await res.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Index));
        }

        var dto = await res.Content.ReadFromJsonAsync<SubmitOrderResponse>();
        return RedirectToAction(nameof(Success), new { id = dto!.Id });
    }


    public IActionResult Success(long id) => View(model: id);
}
