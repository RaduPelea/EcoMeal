using System.Net.Http.Headers;
using Ecomeal.client.Models;

namespace Ecomeal.client.Services;

public class ReviewService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ReviewService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    private async Task EnsureAuthHeaderAsync()
    {
        if (string.IsNullOrEmpty(_auth.Token))
            await _auth.LoadTokenAsync();

        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(_auth.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _auth.Token);
    }

    public async Task<List<ReviewModel>> GetForBusinessAsync(int businessId)
    {
        var reviews = await _http.GetFromJsonAsync<List<ReviewModel>>($"api/review/business/{businessId}");
        return reviews ?? new List<ReviewModel>();
    }

    public async Task<(bool Success, string? Error)> SubmitAsync(ReviewAddModel review)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/review", review);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't save your review. Please try again."));
    }

    public async Task<List<MyReviewModel>> GetMineAsync()
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.GetAsync("api/review/mine");
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<MyReviewModel>>() ?? new();
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/review/{id}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't delete this review. Please try again."));
    }

    public async Task<List<PendingCustomerReviewModel>> GetPendingCustomerReviewsAsync()
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.GetAsync("api/review/customer/pending");
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<PendingCustomerReviewModel>>() ?? new();
    }

    public async Task<(bool Success, string? Error)> SubmitCustomerAsync(CustomerReviewAddModel review)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/review/customer", review);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't save your rating. Please try again."));
    }

    public async Task<bool> CanReviewAsync(int businessId)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.GetAsync($"api/review/eligible/{businessId}");
        if (!response.IsSuccessStatusCode)
            return false;

        return await response.Content.ReadFromJsonAsync<bool>();
    }
}
