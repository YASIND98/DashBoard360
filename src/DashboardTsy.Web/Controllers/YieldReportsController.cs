using DashboardTsy.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers
{
    public class YieldReportsController : Controller
    {
        [HttpGet("verim-raporlari")]
        public IActionResult Index()
        {
            var model = YieldReportDataHelper.GetSampleData();
            return View(model);
        }
    }
}
