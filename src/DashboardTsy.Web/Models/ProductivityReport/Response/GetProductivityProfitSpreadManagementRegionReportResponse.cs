namespace DashboardTsy.Web.Models.ProductivityReport.Response;

public class GetProductivityProfitSpreadManagementRegionReportResponse
{
    public List<GetProductivityProfitSpreadManagementRegionReportItem> GetProductivityProfitSpreadManagementRegionReports { get; set; } = new();

    public class GetProductivityProfitSpreadManagementRegionReportItem
    {
        public string ProductName { get; set; } = string.Empty;

        public int RegionTargetReturnCumulativeCurrentYear { get; set; }
        public int RegionReturnCumulativeCurrentYear { get; set; }
        public int RegionReturnCumulativeLastYear { get; set; }

        public int RegionTargetAverageVolumeCumulativeCurrentYear { get; set; }
        public int RegionAverageVolumeCumulativeCurrentYear { get; set; }
        public int RegionAverageVolumeCumulativeLastYear { get; set; }

        public decimal RegionSpreadTargetCurrentYear { get; set; }
        public decimal RegionSpreadReturnCurrentYear { get; set; }
        public decimal RegionSpreadReturnLastYear { get; set; }

        public decimal BankSpreadTargetCurrentYear { get; set; }
        public decimal BankSpreadReturnCurrentYear { get; set; }
    }
}
