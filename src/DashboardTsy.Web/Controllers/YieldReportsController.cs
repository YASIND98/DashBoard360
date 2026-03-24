using DashboardTsy.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers
{
    public class YieldReportsController : Controller
    {
        [Route("/verim-raporlari")]
        public IActionResult Index()
        {
            var model = new ReportViewModel();
            return View("~/Views/YieldReports/Index.cshtml", model);
        }
    }
}
