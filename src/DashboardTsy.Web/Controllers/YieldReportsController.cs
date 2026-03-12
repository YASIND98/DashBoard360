using DashboardTsy.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers
{
    public class YieldReportsController : Controller
    {
        [Route("/verim-raporlari")]
        public IActionResult Index()
        {
            var model = YieldReportDataHelper.GetSampleData();
            return View("~/Views/YieldReports/Index.cshtml", model);
        }
    }
}
