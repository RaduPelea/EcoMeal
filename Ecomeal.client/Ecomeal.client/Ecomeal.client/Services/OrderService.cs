using Ecomeal.client.Models;

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

    public async Task<(OrderModel? Order, string? Error)> PlaceOrderAsync(int packageId)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/order")
        {
            Content = JsonContent.Create(new { PackageId = packageId })
        };
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return (null, await ApiError.ReadAsync(response, "We couldn't place your order. Please try again."));

        return (await response.Content.ReadFromJsonAsync<OrderModel>(), null);
    }

    public async Task<(bool Success, string? Error)> PlaceBatchAsync(List<int> packageIds, bool useLoyaltyDiscount = false)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/order/batch")
        {
            Content = JsonContent.Create(new { PackageIds = packageIds, UseLoyaltyDiscount = useLoyaltyDiscount })
        };
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't place your order. Please try again."));
    }

    public async Task<OrderModel?> GetByIdAsync(int id)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"api/order/{id}");
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<OrderModel>();
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(int id, string status)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"api/order/{id}/status")
        {
            Content = JsonContent.Create(new { Status = status })
        };
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't update this order. Please try again."));
    }

    public async Task<List<OrderModel>> GetBusinessOrdersAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "api/order/business");
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<OrderModel>>() ?? new();
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

    public async Task<List<PendingReviewModel>> GetPendingReviewsAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "api/order/pending-reviews");
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<PendingReviewModel>>() ?? new();
    }

    public async Task<LoyaltyStatusModel?> GetLoyaltyStatusAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "api/order/loyalty");
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LoyaltyStatusModel>();
    }

    public async Task<(bool Success, string? Error)> ClaimLoyaltyAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/order/loyalty/claim");
        await AddAuthAsync(req);

        var response = await _http.SendAsync(req);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't claim your reward. Please try again."));
    }

    private async Task AddAuthAsync(HttpRequestMessage req)
    {
        if (string.IsNullOrEmpty(_auth.Token))
            await _auth.LoadTokenAsync();

        if (!string.IsNullOrEmpty(_auth.Token))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);
    }
}
