namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityGeneralRegionReportResponse
{
    public List<GetProductivityGeneralRegionReportItem> GetProductivityGeneralRegionReports { get; set; } = new();

    public class GetProductivityGeneralRegionReportItem
    {
        public int Id { get; set; }

        /// <summary>SP'den gelen parent id; null/0 ise root ürün.</summary>
        public int? ParentProductId { get; set; }

        public string Urun { get; set; } = string.Empty;
        public decimal BankaGecenYil { get; set; }
        public decimal BankaGerceklesen { get; set; }
        public decimal BankaOrt { get; set; }
        public decimal BankaHedef { get; set; }
        public decimal HgYuzde { get; set; }
        public decimal NetBuyumeBanka { get; set; }
        public decimal NetBuyumeBankaOrt { get; set; }
        public decimal YtdBanka { get; set; }
        public decimal QtdBanka { get; set; }

        public List<GetProductivityGeneralRegionReportItem> SubProducts { get; set; } = new();
    }
}
