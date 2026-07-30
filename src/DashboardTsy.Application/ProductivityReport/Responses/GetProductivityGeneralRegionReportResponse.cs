namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityGeneralRegionReportResponse
{
    public List<GetProductivityGeneralRegionReportItem> GetProductivityGeneralRegionReports { get; set; } = new();

    public class GetProductivityGeneralRegionReportItem
    {
        public string Urun { get; set; } = string.Empty;
        public decimal BankaGerceklesen { get; set; }
        public decimal BankaOrt { get; set; }
        public decimal BankaHedef { get; set; }
        public decimal HgYuzde { get; set; }
        public decimal NetBuyumeBanka { get; set; }
        public decimal NetBuyumeBankaOrt { get; set; }
        public decimal YtdBanka { get; set; }
        public decimal QtdBanka { get; set; }
    }
}
