using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;
using System.Net.Http.Json; // <— thêm
namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class CategoriesController : Controller
{

    private readonly HttpClient _api;
    public CategoriesController(IHttpClientFactory f) => _api = f.CreateClient("api");
    // ⬇️ helper
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


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormVm vm)
    {
        if (!ModelState.IsValid) { vm.ParentOptions = await LoadParentOptions(null); return View(vm); }

        var dto = new CategoryCreateDto(vm.ParentId, vm.Name, vm.Slug, vm.SortOrder, vm.IsActive);
        var res = await _api.PostAsJsonAsync("/api/admin/categories", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));   // <— dùng helper
            vm.ParentOptions = await LoadParentOptions(null);
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, CategoryFormVm vm)
    {
        if (!ModelState.IsValid) { vm.ParentOptions = await LoadParentOptions(id); return View(vm); }

        var dto = new CategoryUpdateDto(vm.ParentId, vm.Name, vm.Slug, vm.SortOrder, vm.IsActive);
        var res = await _api.PutAsJsonAsync($"/api/admin/categories/{id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ReadApiMessage(res));   // <— dùng helper
            vm.ParentOptions = await LoadParentOptions(id);
            return View(vm);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var data = await _api.GetFromJsonAsync<PagedResult<CategoryDto>>($"/api/admin/categories?q={q}&page={page}&pageSize=100");
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new CategoryFormVm { IsActive = true, SortOrder = 0 };
        vm.ParentOptions = await LoadParentOptions(null);
        return View(vm);
    }

   

    [HttpGet]                      
    public async Task<IActionResult> Edit(long id)
    {
        var dto = await _api.GetFromJsonAsync<CategoryDto>($"/api/admin/categories/{id}");
        if (dto is null) return NotFound();

        var vm = new CategoryFormVm
        {
            Id = dto.Id,
            ParentId = dto.ParentId,
            Name = dto.Name,
            Slug = dto.Slug,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive
        };
        vm.ParentOptions = await LoadParentOptions(id);
        return View(vm);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var res = await _api.DeleteAsync($"/api/admin/categories/{id}");
        // có thể hiển thị TempData nếu muốn
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> LoadParentOptions(long? excludeId)
    {
        // Thêm đúng 1 dòng Root ở ĐÂY (đừng thêm trong View nữa)
        var list = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "— (Root) —" }
    };

        // ❗Chỉ gắn excludeId khi có giá trị để tránh gọi ...?excludeId=
        var url = "/api/admin/categories/options";
        if (excludeId.HasValue)
            url += $"?excludeId={excludeId.Value}";

        var resp = await _api.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return list;

        var opts = await resp.Content.ReadFromJsonAsync<List<CategoryOptionDto>>();
        if (opts != null)
            list.AddRange(opts.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Label
            }));

        return list;
    }



}
