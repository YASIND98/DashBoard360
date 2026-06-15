using DashboardTsy.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers
{
    public class YieldReportsController : Controller
    {
        [Route("/verim-raporlari")]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId <= 0)
                return RedirectToAction("Login", "Auth");

            var model = new ReportViewModel();
            return View("~/Views/YieldReports/Index.cshtml", model);
        }
    }
}
