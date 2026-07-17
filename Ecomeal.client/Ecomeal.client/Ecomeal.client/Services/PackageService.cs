using Ecomeal.client.Models;

namespace Ecomeal.client.Services;

public class PackageService
{
    private readonly HttpClient _http;

    public PackageService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EndingSoonModel>> GetEndingSoonAsync()
    {
        var response = await _http.GetAsync("api/package/ending-soon");
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<EndingSoonModel>>() ?? new();
    }
}
