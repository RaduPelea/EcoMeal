namespace Ecomeal.client.Services;

public class ToastService
{
    public event Action<string, bool>? OnShow;

    public void ShowSuccess(string message) => OnShow?.Invoke(message, true);
    public void ShowError(string message) => OnShow?.Invoke(message, false);
}
