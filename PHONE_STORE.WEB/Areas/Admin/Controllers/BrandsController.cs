using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;
using System.Net.Http.Json;
[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class BrandsController : Controller
{
    private readonly HttpClient _api;
    public BrandsController(IHttpClientFactory f) => _api = f.CreateClient("api");

    // ⬇️ THÊM NGAY TRONG CLASS (ví trí sau constructor)
    private static async Task<string> ReadApiMessage(HttpResponseMessage res)
    {
        try
        {
            var obj = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            if (obj != null && obj.TryGetValue("message", out var m) && !string.IsNullOrWhiteSpace(m))
                return m;
        }
        catch { }
        return await res.Content.ReadAsStringAsync();
    }


    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var res = await _api.GetFromJsonAsync<PagedResult<BrandDto>>($"/api/admin/brands?q={q}&page={page}&pageSize=20");
        return View(res);
    }

    [HttpGet] public IActionResult Create() => View(new BrandFormVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BrandFormVm vm)
    {
        var dto = new BrandCreateDto(vm.Name, vm.Slug, vm.IsActive);
        var res = await _api.PostAsJsonAsync("/api/admin/brands", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var dto = await _api.GetFromJsonAsync<BrandDto>($"/api/admin/brands/{id}");
        if (dto is null) return NotFound();
        return View(new BrandFormVm { Id = dto.Id, Name = dto.Name, Slug = dto.Slug, IsActive = dto.IsActive });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, BrandFormVm vm)
    {
        var dto = new BrandUpdateDto(vm.Name, vm.Slug, vm.IsActive);
        var res = await _api.PutAsJsonAsync($"/api/admin/brands/{id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        await _api.DeleteAsync($"/api/admin/brands/{id}");
        return RedirectToAction(nameof(Index));
    }

}
