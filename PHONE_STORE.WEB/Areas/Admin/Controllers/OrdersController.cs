using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class OrdersController : Controller
{
    private readonly HttpClient _api;
    public OrdersController(IHttpClientFactory f) => _api = f.CreateClient("api");

    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? status)
    {
        var list = await _api.GetFromJsonAsync<List<OrderListItemDto>>(
            $"/api/admin/orders?q={q}&status={status}") ?? new();
        ViewBag.Q = q; ViewBag.Status = status;
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id)
    {
        var d = await _api.GetFromJsonAsync<OrderDetailDto>($"/api/admin/orders/{id}");
        return d == null ? NotFound() : View(d);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(long id)
    {
        var r = await _api.PostAsync($"/api/admin/orders/{id}/mark-paid", null);
        if (!r.IsSuccessStatusCode) TempData["Error"] = await ReadApiMessage(r);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ship(long id)
    {
        var r = await _api.PostAsync($"/api/admin/orders/{id}/ship", null);
        if (!r.IsSuccessStatusCode) TempData["Error"] = await ReadApiMessage(r);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        var r = await _api.PostAsync($"/api/admin/orders/{id}/cancel", null);
        if (!r.IsSuccessStatusCode) TempData["Error"] = await ReadApiMessage(r);
        return RedirectToAction(nameof(Details), new { id });
    }

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

}
