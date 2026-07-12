using System.ComponentModel.DataAnnotations;

namespace EcoMeal.client.Models.Auth;

public class LoginModel
{
    [Required(ErrorMessage = "Email or username is required")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
