using System.Diagnostics;
using DashboardTsy.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("/")]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId <= 0)
                return RedirectToAction("Login", "Auth");

            var model = new ReportViewModel();
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
