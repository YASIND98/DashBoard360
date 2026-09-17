namespace DashboardTsy.Web.Models.BranchMap.Response;

public class GetBranchMapInfoItem
{
    public short RegionCode { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public short BranchCode { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchAddress { get; set; } = string.Empty;
    public decimal Longitude { get; set; }
    public decimal Latitude { get; set; }
}
