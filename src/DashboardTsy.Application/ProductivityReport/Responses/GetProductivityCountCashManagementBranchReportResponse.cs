namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityCountCashManagementBranchReportResponse
{
    public List<GetProductivityCountCashManagementBranchReportItem> GetProductivityCountCashManagementBranchReports { get; set; } = new();

    public class GetProductivityCountCashManagementBranchReportItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationBranchValue { get; set; }
        public decimal RealizationRegionAverageValue { get; set; }
        public decimal? RealizationRegionAverageValueDiff { get; set; }
        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageValueDiff { get; set; }

        public decimal YtdNominalChangeBranchValue { get; set; }
        public decimal YtdNominalChangeRegionAverageValue { get; set; }
        public decimal? YtdNominalChangeRegionAverageValueDiff { get; set; }
        public decimal YtdNominalChangeBankAverageValue { get; set; }
        public decimal? YtdNominalChangeBankAverageValueDiff { get; set; }

        public decimal QtdNominalChangeBranchValue { get; set; }
        public decimal QtdNominalChangeRegionAverageValue { get; set; }
        public decimal? QtdNominalChangeRegionAverageValueDiff { get; set; }
        public decimal QtdNominalChangeBankAverageValue { get; set; }
        public decimal? QtdNominalChangeBankAverageValueDiff { get; set; }

        public List<GetProductivityCountCashManagementBranchReportItem> SubProducts { get; set; } = new();
    }
}
