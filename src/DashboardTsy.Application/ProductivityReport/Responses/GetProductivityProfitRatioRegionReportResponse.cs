namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityProfitRatioRegionReportResponse
{
    public List<GetProductivityProfitRatioRegionReportItem> GetProductivityProfitRatioRegionReports { get; set; } = new();

    public class GetProductivityProfitRatioRegionReportItem
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

        public List<GetProductivityProfitRatioRegionReportItem> SubProducts { get; set; } = new();
    }
}
