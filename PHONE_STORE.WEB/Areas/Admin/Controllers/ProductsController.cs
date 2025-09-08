// PHONE_STORE.WEB/Areas/Admin/Controllers/ProductsController.cs
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class ProductsController : Controller
{
    private readonly HttpClient _api;
    public ProductsController(IHttpClientFactory f) => _api = f.CreateClient("api");

    // helper hiển thị message đẹp
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
    public async Task<IActionResult> Index(string? q, long? brandId, int page = 1)
    {
        var url = $"/api/admin/products?q={q}&brandId={brandId}&page={page}&pageSize=20";
        var data = await _api.GetFromJsonAsync<PagedResult<ProductListItemDto>>(url);
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ProductFormVm { IsActive = true };
        await LoadOptions(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormVm vm)
    {
        if (!ModelState.IsValid) { await LoadOptions(vm); return View(vm); }

        var dto = new ProductCreateDto(vm.BrandId, vm.DefaultCategoryId, vm.Name, vm.Slug, vm.Description, vm.SpecJson, vm.IsActive);
        var res = await _api.PostAsJsonAsync("/api/admin/products", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            await LoadOptions(vm);
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var dto = await _api.GetFromJsonAsync<ProductDto>($"/api/admin/products/{id}");
        if (dto is null) return NotFound();

        var vm = new ProductFormVm
        {
            Id = dto.Id,
            BrandId = dto.BrandId,
            DefaultCategoryId = dto.DefaultCategoryId,
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            SpecJson = dto.SpecJson,
            IsActive = dto.IsActive
        };
        await LoadOptions(vm, excludeCategoryId: null); // options cho Category đã có API tự xử lý exclude nếu cần
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ProductFormVm vm)
    {
        if (!ModelState.IsValid) { await LoadOptions(vm); return View(vm); }

        var dto = new ProductUpdateDto(vm.BrandId, vm.DefaultCategoryId, vm.Name, vm.Slug, vm.Description, vm.SpecJson, vm.IsActive);
        var res = await _api.PutAsJsonAsync($"/api/admin/products/{id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            await LoadOptions(vm);
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        await _api.DeleteAsync($"/api/admin/products/{id}");
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadOptions(ProductFormVm vm, long? excludeCategoryId = null)
    {
        // Brands
        var brandResp = await _api.GetAsync("/api/admin/brands/options");
        var brandOpts = brandResp.IsSuccessStatusCode
            ? await brandResp.Content.ReadFromJsonAsync<List<IdNameDto>>() ?? new()
            : new();

        vm.BrandOptions = brandOpts
            .Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            })
            .ToList();

        // Categories
        var url = excludeCategoryId.HasValue
            ? $"/api/admin/categories/options?excludeId={excludeCategoryId}"
            : "/api/admin/categories/options";

        var catResp = await _api.GetAsync(url);
        var catOpts = catResp.IsSuccessStatusCode
            ? await catResp.Content.ReadFromJsonAsync<List<CategoryOptionDto>>() ?? new()
            : new();

        vm.CategoryOptions = new List<SelectListItem>
    {
        new() { Value = "", Text = "— (Không chọn) —" }
    };

        vm.CategoryOptions.AddRange(catOpts.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Label
        }));
    }

}
