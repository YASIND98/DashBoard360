namespace DashboardTsy.Api.Models.TargetReport;

public class GetDailyTargetReportResponse
{
    public List<Product> Products { get; set; } = new();

    public class Product
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// <summary>SP’den gelen parent id; null/0 ise root ürün.</summary>
        public long? ParentProductId { get; set; }

        public double LastYearAmount { get; set; }
        public DateTime LastYearDate { get; set; }

        public double LastWeekAmount { get; set; }
        public DateTime LastWeekDate { get; set; }

        public double PrevDayAmount { get; set; }
        public DateTime PrevDayDate { get; set; }

        public double YesterdayAmount { get; set; }
        public DateTime YesterdayDate { get; set; }

        public DateTime TodayDate { get; set; }

        public double? DiffByPrevDayAmount { get; set; }
        public double? DiffByLastYearAmount { get; set; }
        public double? DiffByLastWeekAmount { get; set; }

        public List<Product> SubProducts { get; set; } = new();
    }
}

