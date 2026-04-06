namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityCountCardPosRegionReportResponse
{
    public List<GetProductivityCountCardPosRegionReportItem> GetProductivityCountCardPosRegionReports { get; set; } = new();

    public class GetProductivityCountCardPosRegionReportItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public decimal CurrentMonthRegionValue { get; set; }
        public decimal CurrentMonthBankAverage { get; set; }
        public decimal? CurrentMonthBankAverageDiff { get; set; }

        public decimal ThreeMonthHgRegion { get; set; }
        public decimal ThreeMonthHgBankAverage { get; set; }
        public decimal? ThreeMonthHgBankAverageDiff { get; set; }

        public List<GetProductivityCountCardPosRegionReportItem> SubProducts { get; set; } = new();
    }
}
