using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class PricesController : Controller
{
    private readonly HttpClient _api;
    public PricesController(IHttpClientFactory f) => _api = f.CreateClient("api");

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

    // LIST
    [HttpGet]
    public async Task<IActionResult> Index(long variantId, long productId)
    {
        var list = await _api.GetFromJsonAsync<List<PriceDto>>($"/api/admin/variants/{variantId}/prices") ?? new();
        ViewBag.VariantId = variantId;
        ViewBag.ProductId = productId;
        return View(list);
    }

    // CREATE
    [HttpGet]
    public IActionResult Create(long variantId, long productId)
    {
        ViewBag.ProductId = productId;
        return View(new PriceFormVm { VariantId = variantId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PriceFormVm vm, long productId)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = productId;
            return View(vm);
        }

        var dto = new PriceUpsertDto(vm.VariantId, vm.ListPrice, vm.SalePrice, vm.Currency, vm.StartsAt, vm.EndsAt);
        var res = await _api.PostAsJsonAsync($"/api/admin/variants/{vm.VariantId}/prices", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            ViewBag.ProductId = productId;
            return View(vm);
        }

        return RedirectToAction(nameof(Index), new { variantId = vm.VariantId, productId });
    }

    // EDIT
    [HttpGet]
    public async Task<IActionResult> Edit(long id, long variantId, long productId)
    {
        var list = await _api.GetFromJsonAsync<List<PriceDto>>($"/api/admin/variants/{variantId}/prices") ?? new();
        var dto = list.FirstOrDefault(x => x.Id == id);
        if (dto == null) return NotFound();

        var vm = new PriceFormVm
        {
            Id = dto.Id,
            VariantId = variantId,
            ListPrice = dto.ListPrice,
            SalePrice = dto.SalePrice,
            Currency = dto.Currency,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt
        };
        ViewBag.ProductId = productId;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, long variantId, long productId, PriceFormVm vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProductId = productId;
            return View(vm);
        }

        var dto = new PriceUpsertDto(variantId, vm.ListPrice, vm.SalePrice, vm.Currency, vm.StartsAt, vm.EndsAt);
        var res = await _api.PutAsJsonAsync($"/api/admin/variants/{variantId}/prices/{id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            ViewBag.ProductId = productId;
            return View(vm);
        }
        return RedirectToAction(nameof(Index), new { variantId, productId });
    }

    // DELETE
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, long variantId, long productId)
    {
        await _api.DeleteAsync($"/api/admin/variants/{variantId}/prices/{id}");
        return RedirectToAction(nameof(Index), new { variantId, productId });
    }
}
