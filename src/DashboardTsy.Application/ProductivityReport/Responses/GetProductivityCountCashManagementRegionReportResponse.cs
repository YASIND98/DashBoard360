namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityCountCashManagementRegionReportResponse
{
    public List<GetProductivityCountCashManagementRegionReportItem> GetProductivityCountCashManagementRegionReports { get; set; } = new();

    public class GetProductivityCountCashManagementRegionReportItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationRegionValue { get; set; }
        public decimal RealizationRegionAverageValue { get; set; }
        public decimal? RealizationRegionAverageValueDiff { get; set; }
        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageValueDiff { get; set; }

        public decimal YtdNominalChangeRegionValue { get; set; }
        public decimal YtdNominalChangeRegionAverageValue { get; set; }
        public decimal? YtdNominalChangeRegionAverageValueDiff { get; set; }
        public decimal YtdNominalChangeBankAverageValue { get; set; }
        public decimal? YtdNominalChangeBankAverageValueDiff { get; set; }

        public decimal QtdNominalChangeRegionValue { get; set; }
        public decimal QtdNominalChangeRegionAverageValue { get; set; }
        public decimal? QtdNominalChangeRegionAverageValueDiff { get; set; }
        public decimal QtdNominalChangeBankAverageValue { get; set; }
        public decimal? QtdNominalChangeBankAverageValueDiff { get; set; }

        public List<GetProductivityCountCashManagementRegionReportItem> SubProducts { get; set; } = new();
    }
}
