using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos; // PagedResult<BrandDto>
using PHONE_STORE.WEB.Models;        // BrandFormVm

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class BrandsController : Controller
{
    private readonly HttpClient _api;
    public BrandsController(IHttpClientFactory f) => _api = f.CreateClient("api");

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var res = await _api.GetFromJsonAsync<PagedResult<BrandDto>>($"/api/admin/brands?q={q}&page={page}&pageSize=20");
        return View(res);
    }

    [HttpGet] public IActionResult Create() => View(new BrandFormVm());

    [HttpPost]
    public async Task<IActionResult> Create(BrandFormVm vm)
    {
        var dto = new BrandCreateDto(vm.Name, vm.Slug, vm.IsActive);
        var res = await _api.PostAsJsonAsync("/api/admin/brands", dto);
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", "Tạo brand thất bại"); return View(vm); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Edit(long id)
    {
        var dto = await _api.GetFromJsonAsync<BrandDto>($"/api/admin/brands/{id}");
        if (dto is null) return NotFound();
        return View(new BrandFormVm { Id = dto.Id, Name = dto.Name, Slug = dto.Slug, IsActive = dto.IsActive });
    }

    [HttpPost("{id:long}")]
    public async Task<IActionResult> Edit(long id, BrandFormVm vm)
    {
        var dto = new BrandUpdateDto(vm.Name, vm.Slug, vm.IsActive);
        var res = await _api.PutAsJsonAsync($"/api/admin/brands/{id}", dto);
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", "Cập nhật brand thất bại"); return View(vm); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:long}/delete")]
    public async Task<IActionResult> Delete(long id)
    {
        await _api.DeleteAsync($"/api/admin/brands/{id}");
        return RedirectToAction(nameof(Index));
    }
}
