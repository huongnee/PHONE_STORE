using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
