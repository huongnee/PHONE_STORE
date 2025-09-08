using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class InventoryController : Controller
{
    private readonly HttpClient _api;
    public InventoryController(IHttpClientFactory f) => _api = f.CreateClient("api");

    [HttpGet]
    public async Task<IActionResult> Move(long? variantId)
    {
        var vm = new StockMoveVm { VariantId = variantId ?? 0, MovementType = "IN", Quantity = 1 };
        var whs = await _api.GetFromJsonAsync<List<WarehouseOptionDto>>("/api/admin/warehouses/options") ?? new();
        vm.WarehouseOptions = whs.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Label }).ToList();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(StockMoveVm vm)
    {
        if (!ModelState.IsValid)
        {
            var whs = await _api.GetFromJsonAsync<List<WarehouseOptionDto>>("/api/admin/warehouses/options") ?? new();
            vm.WarehouseOptions = whs.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Label }).ToList();
            return View(vm);
        }

        var imeis = string.IsNullOrWhiteSpace(vm.ImeiCsv)
            ? null
            : vm.ImeiCsv
                .Replace("\r", "")
                .Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => s.Length > 0).Distinct().ToList();

        var req = new StockMoveRequestDto(vm.VariantId, vm.WarehouseId, vm.MovementType, vm.Quantity,
            vm.RefType, vm.RefId, vm.RefCode, vm.Note, imeis);

        var res = await _api.PostAsJsonAsync("/api/admin/inventory/move", req);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            ModelState.AddModelError("", err is not null && err.TryGetValue("message", out var msg) ? msg : "Error");
            var whs = await _api.GetFromJsonAsync<List<WarehouseOptionDto>>("/api/admin/warehouses/options") ?? new();
            vm.WarehouseOptions = whs.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Label }).ToList();
            return View(vm);
        }
        TempData["ok"] = "Đã ghi nhận phiếu kho.";
        return RedirectToAction(nameof(Move), new { variantId = vm.VariantId });
    }
    // ===== Inventory listing: by Variant =====
    [HttpGet]
    public async Task<IActionResult> ByVariant(long? variantId)
    {
        ViewBag.VariantId = variantId;
        if (variantId is null || variantId == 0)
            return View(new List<InventoryDto>());

        var rows = await _api.GetFromJsonAsync<List<InventoryDto>>($"/api/admin/inventory?variantId={variantId}");
        var whs = await _api.GetFromJsonAsync<List<WarehouseOptionDto>>("/api/admin/warehouses/options") ?? new();
        ViewBag.WarehouseDict = whs.ToDictionary(x => x.Id, x => x.Label);
        return View(rows ?? new List<InventoryDto>());
    }

    // ===== Inventory listing: by Warehouse =====
    [HttpGet]
    public async Task<IActionResult> ByWarehouse(long? warehouseId)
    {
        var whs = await _api.GetFromJsonAsync<List<WarehouseOptionDto>>("/api/admin/warehouses/options") ?? new();
        ViewBag.WarehouseOptions = whs;
        ViewBag.WarehouseId = warehouseId;

        if (warehouseId is null || warehouseId == 0)
            return View(new List<InventoryDto>());

        var rows = await _api.GetFromJsonAsync<List<InventoryDto>>($"/api/admin/inventory?warehouseId={warehouseId}");
        return View(rows ?? new List<InventoryDto>());
    }


}