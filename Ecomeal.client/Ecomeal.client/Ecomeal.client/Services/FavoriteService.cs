using System.Net.Http.Headers;
using Ecomeal.client.Models;

namespace Ecomeal.client.Services;

public class FavoriteService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public FavoriteService(HttpClient http, AuthService auth)
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

    public async Task<List<BusinessModel>> GetMyFavoritesAsync()
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.GetAsync("api/favorite");
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<BusinessModel>>() ?? new();
    }

    public async Task<HashSet<int>> GetMyFavoriteIdsAsync()
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.GetAsync("api/favorite/ids");
        if (!response.IsSuccessStatusCode)
            return new();

        var ids = await response.Content.ReadFromJsonAsync<List<int>>();
        return ids is null ? new() : new HashSet<int>(ids);
    }

    public async Task<(bool? IsFavorite, string? Error)> ToggleAsync(int businessId)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsync($"api/favorite/{businessId}", null);
        if (!response.IsSuccessStatusCode)
            return (null, await ApiError.ReadAsync(response, "We couldn't update your favorites. Please try again."));

        var state = await response.Content.ReadFromJsonAsync<FavoriteStateModel>();
        return (state?.IsFavorite, null);
    }
}
