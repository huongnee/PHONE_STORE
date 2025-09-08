using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class ImagesController : Controller
{
    private readonly HttpClient _api;
    public ImagesController(IHttpClientFactory f) => _api = f.CreateClient("api");

    private static async Task<string> ReadApiMessage(HttpResponseMessage res)
    {
        try
        {
            var obj = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (obj != null && obj.TryGetValue("message", out var m) && !string.IsNullOrWhiteSpace(m)) return m;
        }
        catch { }
        return await res.Content.ReadAsStringAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Product(long productId)
    {
        var imgs = await _api.GetFromJsonAsync<List<ImageDto>>($"/api/admin/images/product/{productId}") ?? new();
        ViewBag.ProductId = productId;
        return View("Product", imgs);
    }

    [HttpGet]
    public async Task<IActionResult> Variant(long variantId)
    {
        var imgs = await _api.GetFromJsonAsync<List<ImageDto>>($"/api/admin/images/variant/{variantId}") ?? new();
        ViewBag.VariantId = variantId;
        return View("Variant", imgs);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToProduct(ImageFormVm vm)
    {
        var dto = new ImageCreateDto(vm.ProductId, null, vm.ImageUrl, vm.AltText, vm.IsPrimary, vm.SortOrder);
        var res = await _api.PostAsJsonAsync("/api/admin/images", dto);
        if (!res.IsSuccessStatusCode) TempData["Error"] = await ReadApiMessage(res);
        return RedirectToAction(nameof(Product), new { productId = vm.ProductId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToVariant(ImageFormVm vm)
    {
        var dto = new ImageCreateDto(null, vm.VariantId, vm.ImageUrl, vm.AltText, vm.IsPrimary, vm.SortOrder);
        var res = await _api.PostAsJsonAsync("/api/admin/images", dto);
        if (!res.IsSuccessStatusCode) TempData["Error"] = await ReadApiMessage(res);
        return RedirectToAction(nameof(Variant), new { variantId = vm.VariantId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, long? productId, long? variantId)
    {
        await _api.DeleteAsync($"/api/admin/images/{id}");
        if (productId.HasValue) return RedirectToAction(nameof(Product), new { productId });
        if (variantId.HasValue) return RedirectToAction(nameof(Variant), new { variantId });
        return RedirectToAction(nameof(Product));
    }
}
