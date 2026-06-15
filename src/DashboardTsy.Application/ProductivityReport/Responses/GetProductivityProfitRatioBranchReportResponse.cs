namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityProfitRatioBranchReportResponse
{
    public List<GetProductivityProfitRatioBranchReportItem> GetProductivityProfitRatioBranchReports { get; set; } = new();

    public class GetProductivityProfitRatioBranchReportItem
    {
        public int Id { get; set; }
        public string RatioName { get; set; } = string.Empty;

        public decimal TargetValue { get; set; }

        public decimal RegionValue { get; set; }
        public decimal? RegionValueDiff { get; set; }

        public decimal BankValue { get; set; }
        public decimal? BankValueDiff { get; set; }

        public decimal RetailValue { get; set; }
        public decimal KobiValue { get; set; }

        public decimal AgricultureValue { get; set; }
        public decimal? AgricultureValueDiff { get; set; }

        public decimal CommercialValue { get; set; }
        public decimal? CommercialValueDiff { get; set; }

        public List<GetProductivityProfitRatioBranchReportItem> SubProducts { get; set; } = new();
    }
}
