namespace Ecomeal.client.Models;

public class PendingReviewModel
{
    public int OrderId { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public string PackageName { get; set; } = "";
}
