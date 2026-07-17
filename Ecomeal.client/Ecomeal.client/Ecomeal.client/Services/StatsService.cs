using System.Net.Http.Headers;
using Ecomeal.client.Models;

namespace Ecomeal.client.Services;

public class StatsService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public StatsService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<StatsModel?> GetAsync()
    {
        if (string.IsNullOrEmpty(_auth.Token))
            await _auth.LoadTokenAsync();

        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(_auth.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _auth.Token);

        var response = await _http.GetAsync("api/stats");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<StatsModel>();
    }

    public async Task<BusinessStatsModel?> GetBusinessStatsAsync()
    {
        if (string.IsNullOrEmpty(_auth.Token))
            await _auth.LoadTokenAsync();

        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(_auth.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _auth.Token);

        var response = await _http.GetAsync("api/stats/business");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BusinessStatsModel>();
    }
}
