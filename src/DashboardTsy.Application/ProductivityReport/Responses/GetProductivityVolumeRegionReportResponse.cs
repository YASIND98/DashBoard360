namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityVolumeRegionReportResponse
{
    public List<GetProductivityVolumeRegionReportItem> GetProductivityVolumeRegionReports { get; set; } = new();

    public class GetProductivityVolumeRegionReportItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationRegionValue { get; set; }
        public decimal? RealizationRegionDiff { get; set; }

        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageDiff { get; set; }

        public decimal TargetValue { get; set; }
        public decimal HgRate { get; set; }

        public decimal NetGrowthRegionValue { get; set; }
        public decimal? NetGrowthRegionDiff { get; set; }

        public decimal NetGrowthBankAverageValue { get; set; }
        public decimal? NetGrowthBankAverageDiff { get; set; }

        public decimal YtdRegionValue { get; set; }
        public decimal? YtdRegionDiff { get; set; }

        public decimal YtdBankAverageValue { get; set; }
        public decimal? YtdBankAverageDiff { get; set; }

        public decimal QtdRegionValue { get; set; }
        public decimal? QtdRegionDiff { get; set; }

        public decimal QtdBankAverageValue { get; set; }
        public decimal? QtdBankAverageDiff { get; set; }

        public List<GetProductivityVolumeRegionReportItem> SubProducts { get; set; } = new();
    }
}
