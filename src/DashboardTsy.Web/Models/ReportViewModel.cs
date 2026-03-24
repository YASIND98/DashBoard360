namespace DashboardTsy.Web.Models;

public class ReportViewModel
{
    public string Tarih { get; set; } = DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
}
