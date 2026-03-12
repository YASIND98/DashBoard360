namespace DashboardTsy.Web.Models;

public class ApiResponse<T> where T : class?
{
    public T? Result { get; set; }
    public MessageResult? Message { get; set; }
}
