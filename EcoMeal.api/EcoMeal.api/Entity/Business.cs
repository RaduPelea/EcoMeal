namespace EcoMeal.api.Models;

public class Business
{
    public int Id { get; set; }
    public string Adress { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Businiess_TypeId { get; set; }
    public string Contact { get; set; }
    public float Rating { get; set; }
}