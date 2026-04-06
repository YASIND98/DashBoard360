namespace DashboardTsy.Web.Models.ProductivityReport.Response;

public class GetProductivityProfitTotalBranchReportResponse
{
    public List<GetProductivityProfitTotalBranchReportItem> GetProductivityProfitTotalBranchReports { get; set; } = new();

    public class GetProductivityProfitTotalBranchReportItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;

        public decimal TargetValue { get; set; }

        public decimal RealizationBranchValue { get; set; }
        public decimal RealizationRegionAverageValue { get; set; }
        public decimal? RealizationRegionAverageValueDiff { get; set; }
        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageValueDiff { get; set; }

        public decimal HgBranchValue { get; set; }
        public decimal? HgBranchValueDiff { get; set; }
        public decimal HgRegionAverageValue { get; set; }
        public decimal? HgRegionAverageValueDiff { get; set; }
        public decimal HgBankAverageValue { get; set; }
        public decimal? HgBankAverageValueDiff { get; set; }

        public decimal RetailValue { get; set; }
        public decimal KobiValue { get; set; }
        public decimal AgricultureValue { get; set; }
        public decimal CommercialValue { get; set; }
        public decimal? CommercialValueDiff { get; set; }
        public decimal PartnerValue { get; set; }

        public List<GetProductivityProfitTotalBranchReportItem> SubProducts { get; set; } = new();
    }
}
