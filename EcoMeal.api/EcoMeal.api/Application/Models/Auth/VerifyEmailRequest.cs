namespace EcoMeal.api.Application.Models.Auth;

public class VerifyEmailRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}

public class ResendCodeRequest
{
    public required string Email { get; set; }
}
