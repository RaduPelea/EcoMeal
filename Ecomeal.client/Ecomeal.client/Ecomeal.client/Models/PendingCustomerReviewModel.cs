namespace Ecomeal.client.Models;

public class PendingCustomerReviewModel
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public string PackageName { get; set; } = "";
}
