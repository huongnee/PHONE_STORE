using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class AttributesController : Controller
{
    private readonly HttpClient _api;
    public AttributesController(IHttpClientFactory f) => _api = f.CreateClient("api");

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

    // ===== Master list =====
    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await _api.GetFromJsonAsync<List<AttributeDto>>("/api/admin/attributes") ?? new());

    [HttpGet]
    public IActionResult Create() => View(new AttributeCreateDto("", "", "TEXT"));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttributeCreateDto dto)
    {
        var res = await _api.PostAsJsonAsync("/api/admin/attributes", dto);
        if (!res.IsSuccessStatusCode) { TempData["Error"] = await ReadApiMessage(res); return View(dto); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var list = await _api.GetFromJsonAsync<List<AttributeDto>>("/api/admin/attributes") ?? new();
        var dto = list.FirstOrDefault(x => x.Id == id);
        if (dto == null) return NotFound();
        return View(new AttributeUpdateDto(dto.Name, dto.DataType));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, AttributeUpdateDto dto)
    {
        var res = await _api.PutAsJsonAsync($"/api/admin/attributes/{id}", dto);
        if (!res.IsSuccessStatusCode) { TempData["Error"] = await ReadApiMessage(res); return View(dto); }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        await _api.DeleteAsync($"/api/admin/attributes/{id}");
        return RedirectToAction(nameof(Index));
    }

    // ===== Product values =====
    public record ProductAttrValueDto(AttributeDto Attr, object? Value);

    [HttpGet]
    public async Task<IActionResult> ProductValues(long productId)
    {
        var all = await _api.GetFromJsonAsync<List<AttributeDto>>("/api/admin/attributes") ?? new();
        var vals = await _api.GetFromJsonAsync<List<ProductAttrValueDto>>($"/api/admin/products/{productId}/attributes") ?? new();
        ViewBag.ProductId = productId;
        ViewBag.AllAttributes = all;
        return View(vals);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpsertValue(long productId, long attributeId, string dataType, long? intValue, decimal? decValue, bool? boolValue, string? textValue)
    {
        var dto = new AttributeValueUpsertDto(attributeId, intValue, decValue, boolValue, textValue);
        var res = await _api.PostAsJsonAsync($"/api/admin/products/{productId}/attributes/upsert", dto);
        if (!res.IsSuccessStatusCode) TempData["Error"] = await ReadApiMessage(res);
        return RedirectToAction(nameof(ProductValues), new { productId });
    }
}
