namespace DashboardTsy.Web.Models.ProductivityReport.Response;

public class GetProductivityCountCustomerRegionReportResponse
{
    public List<GetProductivityCountCustomerRegionReportItem> GetProductivityCountCustomerRegionReports { get; set; } = new();

    public class GetProductivityCountCustomerRegionReportItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationRegion { get; set; }
        public decimal? RealizationRegionDiff { get; set; }
        public decimal RealizationBankAverage { get; set; }
        public decimal? RealizationBankAverageDiff { get; set; }

        public decimal YtdChangeRegion { get; set; }
        public decimal? YtdChangeRegionDiff { get; set; }
        public decimal YtdChangeBankAverage { get; set; }
        public decimal? YtdChangeBankAverageDiff { get; set; }

        public decimal QtdChangeRegion { get; set; }
        public decimal? QtdChangeRegionDiff { get; set; }
        public decimal QtdChangeBankAverage { get; set; }
        public decimal? QtdChangeBankAverageDiff { get; set; }

        public List<GetProductivityCountCustomerRegionReportItem> SubProducts { get; set; } = new();
    }
}
