using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.WEB.Models.Shop;

public class CatalogController : Controller
{
    private readonly HttpClient _api;
    public CatalogController(IHttpClientFactory f) => _api = f.CreateClient("api");

    public async Task<IActionResult> Index(string? q, long? catId, int page = 1)
    {
        var list = await _api.GetFromJsonAsync<List<ProductListItemVm>>(
            $"/api/shop/products?q={q}&catId={catId}&page={page}") ?? new();
        ViewBag.Q = q; ViewBag.CatId = catId;
        return View(list);
    }

    [HttpGet("/p/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var d = await _api.GetFromJsonAsync<ProductDetailVm>($"/api/shop/products/{slug}");
        if (d is null) return NotFound();
        return View(d);
    }
}
