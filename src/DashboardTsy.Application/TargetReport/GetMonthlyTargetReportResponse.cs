namespace DashboardTsy.Application.TargetReport;

public class GetMonthlyTargetReportResponse
{
    public List<Product> Products { get; set; } = new();

    public class Product
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// <summary>SP’den gelen parent id; null/0 ise root ürün.</summary>
        public long? ParentProductId { get; set; }

        public double MonthActualAmount { get; set; }
        public double MonthTargetAmount { get; set; }
        public double MonthRatio { get; set; }

        public double YearActualAmount { get; set; }
        public double YearTargetAmount { get; set; }
        public double YearRatio { get; set; }

        public List<Product> SubProducts { get; set; } = new();
    }
}

