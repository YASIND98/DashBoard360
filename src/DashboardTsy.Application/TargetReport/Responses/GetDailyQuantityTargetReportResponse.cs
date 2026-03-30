namespace DashboardTsy.Application.TargetReport.Responses;

public class GetDailyQuantityTargetReportResponse
{
    public List<Product> Products { get; set; } = new();

    public class Product
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int LevelNo { get; set; }
        public int SortOrder { get; set; }

        public double LastYearAmount { get; set; }
        public DateTime LastYearDate { get; set; }

        public double LastMonthAmount { get; set; }
        public DateTime LastMonthDate { get; set; }

        public double LastTwoMonthEarlierAmount { get; set; }
        public DateTime LastTwoMonthEarlierDate { get; set; }

        public DateTime TodayDate { get; set; }

        public double? DiffByLastTwoMonthEarlierAmount { get; set; }
        public double? DiffByLastYearAmount { get; set; }
        public double? DiffByLastMonthAmount { get; set; }

        public List<Product> SubProducts { get; set; } = new();
    }
}
