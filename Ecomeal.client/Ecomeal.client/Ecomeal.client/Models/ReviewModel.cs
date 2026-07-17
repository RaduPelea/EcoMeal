namespace Ecomeal.client.Models;

public class ReviewModel
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime Date { get; set; }
}
