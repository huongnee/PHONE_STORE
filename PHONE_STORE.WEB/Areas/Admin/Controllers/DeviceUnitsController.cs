using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class DeviceUnitsController : Controller
{
    private readonly HttpClient _api;
    public DeviceUnitsController(IHttpClientFactory f) => _api = f.CreateClient("api");

    [HttpGet]
    public async Task<IActionResult> Index(string? imei, long? variantId, long? warehouseId)
    {
        ViewBag.Imei = imei;
        ViewBag.VariantId = variantId;
        ViewBag.WarehouseId = warehouseId;

        if (string.IsNullOrWhiteSpace(imei) && !variantId.HasValue && !warehouseId.HasValue)
            return View(new List<DeviceUnitDto>());

        var url = $"/api/admin/device-units?imei={imei}&variantId={variantId}&warehouseId={warehouseId}";
        var data = await _api.GetFromJsonAsync<List<DeviceUnitDto>>(url);
        return View(data ?? new List<DeviceUnitDto>());
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        var d = await _api.GetFromJsonAsync<DeviceUnitDto>($"/api/admin/device-units/{id}");
        if (d is null) return NotFound();
        return View(d);
    }
}
