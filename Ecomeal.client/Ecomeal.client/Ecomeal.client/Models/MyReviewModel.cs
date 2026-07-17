namespace Ecomeal.client.Models;

public class MyReviewModel
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime Date { get; set; }
}
