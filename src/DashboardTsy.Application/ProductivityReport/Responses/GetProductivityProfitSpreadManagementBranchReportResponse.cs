namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityProfitSpreadManagementBranchReportResponse
{
    public List<GetProductivityProfitSpreadManagementBranchReportItem> GetProductivityProfitSpreadManagementBranchReports { get; set; } = new();

    public class GetProductivityProfitSpreadManagementBranchReportItem
    {
        public string ProductName { get; set; } = string.Empty;

        public int BranchTargetReturnCumulativeCurrentYear { get; set; }
        public int BranchReturnCumulativeCurrentYear { get; set; }
        public int BranchReturnCumulativeLastYear { get; set; }

        public int BranchTargetAverageVolumeCumulativeCurrentYear { get; set; }
        public int BranchAverageVolumeCumulativeCurrentYear { get; set; }
        public int BranchAverageVolumeCumulativeLastYear { get; set; }

        public decimal BranchSpreadTargetCurrentYear { get; set; }
        public decimal BranchSpreadReturnCurrentYear { get; set; }
        public decimal BranchSpreadReturnLastYear { get; set; }

        public decimal RegionSpreadTargetCurrentYear { get; set; }
        public decimal RegionSpreadReturnCurrentYear { get; set; }

        public decimal BankSpreadTargetCurrentYear { get; set; }
        public decimal BankSpreadReturnCurrentYear { get; set; }
    }
}
