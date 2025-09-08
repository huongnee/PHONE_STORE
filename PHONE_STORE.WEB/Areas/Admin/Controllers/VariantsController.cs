using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class VariantsController : Controller
{
    private readonly HttpClient _api;
    public VariantsController(IHttpClientFactory f) => _api = f.CreateClient("api");

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
    public async Task<IActionResult> Index(long productId)
    {
        var list = await _api.GetFromJsonAsync<List<VariantDto>>($"/api/admin/products/{productId}/variants") ?? new();
        ViewBag.ProductId = productId;
        return View(list);
    }

    [HttpGet]
    public IActionResult Create(long productId)
        => View(new VariantFormVm { ProductId = productId, IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VariantFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var dto = new VariantCreateDto(vm.ProductId, vm.Sku, vm.Color, vm.StorageGb, vm.Barcode, vm.WeightGram, vm.IsActive);
        var res = await _api.PostAsJsonAsync($"/api/admin/products/{vm.ProductId}/variants", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            return View(vm);
        }
        return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, long productId)
    {
        var dto = await _api.GetFromJsonAsync<VariantDto>($"/api/admin/products/{productId}/variants/{id}");
        if (dto is null) return NotFound();
        var vm = new VariantFormVm
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            Sku = dto.Sku,
            Color = dto.Color,
            StorageGb = dto.StorageGb,
            Barcode = dto.Barcode,
            WeightGram = dto.WeightGram,
            IsActive = dto.IsActive
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, long productId, VariantFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var dto = new VariantUpdateDto(vm.Sku, vm.Color, vm.StorageGb, vm.Barcode, vm.WeightGram, vm.IsActive);
        var res = await _api.PutAsJsonAsync($"/api/admin/products/{productId}/variants/{id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            return View(vm);
        }
        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, long productId)
    {
        await _api.DeleteAsync($"/api/admin/products/{productId}/variants/{id}");
        return RedirectToAction(nameof(Index), new { productId });
    }
}
