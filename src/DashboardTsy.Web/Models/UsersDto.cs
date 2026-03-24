namespace DashboardTsy.Web.Models;

public class UsersDto
{
    public int UserId { get; set; }
    public string? NameSurname { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedDate { get; set; }
    public int Type { get; set; }
    public bool PassChange { get; set; }
    public string? ProfilePhoto { get; set; }
    public string? Department { get; set; }
    public int? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? Authority { get; set; }
    public int? RegionCode { get; set; }
    public string? DomainName { get; set; }
    public bool? IsBlock { get; set; }
    public bool? UpdateSeen { get; set; }
}
