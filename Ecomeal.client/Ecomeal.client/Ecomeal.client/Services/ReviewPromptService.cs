namespace Ecomeal.client.Services;

public class ReviewPromptService
{
    public event Action? OnCheck;

    public void RequestCheck()
    {
        OnCheck?.Invoke();
    }
}
