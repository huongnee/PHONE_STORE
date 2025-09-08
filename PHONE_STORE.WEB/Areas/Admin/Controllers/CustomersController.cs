using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class CustomersController : Controller
{
    private readonly HttpClient _api;
    public CustomersController(IHttpClientFactory f) => _api = f.CreateClient("api");

    private static async Task<string> ReadApiMsg(HttpResponseMessage res)
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
    public async Task<IActionResult> Index(string? q)
    {
        var data = await _api.GetFromJsonAsync<List<CustomerDto>>($"/api/admin/customers?q={q}");
        ViewBag.Q = q;
        return View(data ?? new List<CustomerDto>());
    }

    [HttpGet]
    public IActionResult Create() => View(new CustomerFormVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormVm vm)
    {
        var dto = new CustomerUpsertDto(vm.UserAccountId, vm.Email, vm.Phone, vm.FullName);
        var res = await _api.PostAsJsonAsync("/api/admin/customers", dto);
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", await ReadApiMsg(res)); return View(vm); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var d = await _api.GetFromJsonAsync<CustomerDto>($"/api/admin/customers/{id}");
        if (d is null) return NotFound();
        return View(new CustomerFormVm { Id = d.Id, UserAccountId = d.UserAccountId, Email = d.Email, Phone = d.Phone, FullName = d.FullName });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, CustomerFormVm vm)
    {
        var dto = new CustomerUpsertDto(vm.UserAccountId, vm.Email, vm.Phone, vm.FullName);
        var res = await _api.PutAsJsonAsync($"/api/admin/customers/{id}", dto);
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", await ReadApiMsg(res)); return View(vm); }
        return RedirectToAction(nameof(Index));
    }

    // ----- Addresses -----
    [HttpGet]
    public async Task<IActionResult> Addresses(long id)
    {
        var cus = await _api.GetFromJsonAsync<CustomerDto>($"/api/admin/customers/{id}");
        if (cus is null) return NotFound();
        var rows = await _api.GetFromJsonAsync<List<AddressDto>>($"/api/admin/customers/{id}/addresses");
        ViewBag.Customer = cus;
        return View(rows ?? new List<AddressDto>());
    }

    [HttpGet]
    public IActionResult AddAddress(long id)
        => View(new AddressFormVm { CustomerId = id, AddressType = "SHIPPING", IsDefault = false });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(AddressFormVm vm)
    {
        var dto = new AddressUpsertDto(vm.Label, vm.Recipient, vm.Phone, vm.Line1, vm.Ward, vm.District, vm.Province, vm.PostalCode, vm.AddressType, vm.IsDefault);
        var res = await _api.PostAsJsonAsync($"/api/admin/customers/{vm.CustomerId}/addresses", dto);
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", await ReadApiMsg(res)); return View(vm); }
        return RedirectToAction(nameof(Addresses), new { id = vm.CustomerId });
    }

    [HttpGet]
    public async Task<IActionResult> EditAddress(long id, long addressId)
    {
        var d = await _api.GetFromJsonAsync<AddressDto>($"/api/admin/customers/{id}/addresses/{addressId}");
        if (d is null) return NotFound();
        return View(new AddressFormVm
        {
            Id = d.Id,
            CustomerId = d.CustomerId,
            Label = d.Label,
            Recipient = d.Recipient,
            Phone = d.Phone,
            Line1 = d.Line1,
            Ward = d.Ward,
            District = d.District,
            Province = d.Province,
            PostalCode = d.PostalCode,
            AddressType = d.AddressType,
            IsDefault = d.IsDefault
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(AddressFormVm vm)
    {
        var dto = new AddressUpsertDto(vm.Label, vm.Recipient, vm.Phone, vm.Line1, vm.Ward, vm.District, vm.Province, vm.PostalCode, vm.AddressType, vm.IsDefault);
        var res = await _api.PutAsJsonAsync($"/api/admin/customers/{vm.CustomerId}/addresses/{vm.Id}", dto);
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", await ReadApiMsg(res)); return View(vm); }
        return RedirectToAction(nameof(Addresses), new { id = vm.CustomerId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(long id, long addressId, string type)
    {
        await _api.PostAsync($"/api/admin/customers/{id}/addresses/{addressId}/set-default?type={type}", null);
        return RedirectToAction(nameof(Addresses), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(long id, long addressId)
    {
        await _api.DeleteAsync($"/api/admin/customers/{id}/addresses/{addressId}");
        return RedirectToAction(nameof(Addresses), new { id });
    }
}