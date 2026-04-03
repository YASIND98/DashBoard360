namespace DashboardTsy.Web.Models.ProductivityReport;

public class GetProductivityProfitSpreadManagementRegionReportResponse
{
    public List<GetProductivityProfitSpreadManagementRegionReportItem> GetProductivityProfitSpreadManagementRegionReports { get; set; } = new();

    public class GetProductivityProfitSpreadManagementRegionReportItem
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal SpreadValue { get; set; }

        public decimal RatioRegionValue { get; set; }
        public decimal? RatioRegionValueDiff { get; set; }
        public decimal RatioBankAverageValue { get; set; }
        public decimal? RatioBankAverageValueDiff { get; set; }

        public decimal NetReturnRegionValue { get; set; }
        public decimal? NetReturnRegionValueDiff { get; set; }
        public decimal NetReturnBankAverageValue { get; set; }
        public decimal? NetReturnBankAverageValueDiff { get; set; }
        public decimal NetReturnHgRegionValue { get; set; }
        public decimal? NetReturnHgRegionValueDiff { get; set; }
        public decimal NetReturnHgBankAverageValue { get; set; }
        public decimal? NetReturnHgBankAverageValueDiff { get; set; }

        public List<GetProductivityProfitSpreadManagementRegionReportItem> SubProducts { get; set; } = new();
    }
}
