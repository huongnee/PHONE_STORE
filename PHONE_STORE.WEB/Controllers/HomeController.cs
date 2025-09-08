using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.WEB.Models;
using System.Diagnostics;

namespace PHONE_STORE.WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        [HttpGet]
        public IActionResult Index() => RedirectToAction("Index", "Catalog"); // chuy?n th?ng sang trang shop

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}

