using Ecomeal.client.Models;
using EcoMeal.client.Services;

namespace Ecomeal.client.Services;

public class OrderService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public OrderService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<bool> PlaceOrderAsync(int packageId)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/order")
        {
            Content = JsonContent.Create(new { PackageId = packageId })
        };
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<OrderModel>> GetMyOrdersAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "api/order");
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new();
    }

    private async Task AddAuthAsync(HttpRequestMessage req)
    {
        if (string.IsNullOrEmpty(_auth.Token))
            await _auth.LoadTokenAsync();

        if (!string.IsNullOrEmpty(_auth.Token))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);
    }
}
