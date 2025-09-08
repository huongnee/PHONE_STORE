using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class WarehousesController : Controller
{
    private readonly HttpClient _api;
    public WarehousesController(IHttpClientFactory f) => _api = f.CreateClient("api");

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
    public async Task<IActionResult> Index()
    {
        var data = await _api.GetFromJsonAsync<List<WarehouseDto>>("/api/admin/warehouses");
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(new WarehouseFormVm { IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WarehouseFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var dto = new WarehouseUpsertDto(vm.Code, vm.Name, vm.AddressLine, vm.District, vm.Province, vm.IsActive);
        var res = await _api.PostAsJsonAsync("/api/admin/warehouses", dto);
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
        var dto = await _api.GetFromJsonAsync<WarehouseDto>($"/api/admin/warehouses/{id}");
        if (dto is null) return NotFound();
        var vm = new WarehouseFormVm
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            AddressLine = dto.AddressLine,
            District = dto.District,
            Province = dto.Province,
            IsActive = dto.IsActive
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, WarehouseFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var dto = new WarehouseUpsertDto(vm.Code, vm.Name, vm.AddressLine, vm.District, vm.Province, vm.IsActive);
        var res = await _api.PutAsJsonAsync($"/api/admin/warehouses/{id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }
}