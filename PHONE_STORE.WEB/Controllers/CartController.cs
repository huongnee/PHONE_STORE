using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos; // CartDto

public class CartController : Controller
{
    private readonly HttpClient _api;
    public CartController(IHttpClientFactory f) => _api = f.CreateClient("api");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _api.GetFromJsonAsync<CartDto>("/api/shop/cart");
        return View(cart);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(long variantId, int qty = 1, string? returnUrl = null)
    {
        await _api.PostAsJsonAsync("/api/shop/cart/items", new { VariantId = variantId, Quantity = qty });
        return Redirect(returnUrl ?? Url.Action("Index", "Cart")!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long itemId, int qty)
    {
        await _api.PutAsync($"/api/shop/cart/items/{itemId}?qty={qty}", null);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(long itemId)
    {
        await _api.DeleteAsync($"/api/shop/cart/items/{itemId}");
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var cart = await _api.GetFromJsonAsync<PHONE_STORE.Application.Dtos.CartDto>("/api/shop/cart");
        var count = cart?.Items?.Sum(i => i.Quantity) ?? 0;
        return Json(new { count });
    }

}
