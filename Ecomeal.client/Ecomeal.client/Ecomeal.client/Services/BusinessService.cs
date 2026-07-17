using System.Net.Http.Headers;
using Ecomeal.client.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Ecomeal.client.Services;

// serviciul care vorbeste utilizatorul cu api prin hhttp

public class BusinessService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public BusinessService(HttpClient http, AuthService auth)
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

    // read: cere lista de afaceri de la api 
    public async Task<List<BusinessModel>> GetAllAsync()
    {
        var businesses = await _http.GetFromJsonAsync<List<BusinessModel>>("api/business");
        return businesses ?? new List<BusinessModel>();
    }

    public async Task<BusinessPageModel> BrowseAsync(string? type, string? city, int page, int pageSize)
    {
        var url = $"api/business/browse?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(type))
            url += $"&type={Uri.EscapeDataString(type)}";
        if (!string.IsNullOrWhiteSpace(city))
            url += $"&city={Uri.EscapeDataString(city)}";

        var result = await _http.GetFromJsonAsync<BusinessPageModel>(url);
        return result ?? new BusinessPageModel();
    }

    public async Task<List<SearchResultModel>> SearchAsync(string query)
    {
        var results = await _http.GetFromJsonAsync<List<SearchResultModel>>($"api/search?q={Uri.EscapeDataString(query)}");
        return results ?? new List<SearchResultModel>();
    }

    // distinct cities that have restaurants (for the city search)
    public async Task<List<string>> GetCitiesAsync()
    {
        var cities = await _http.GetFromJsonAsync<List<string>>("api/business/cities");
        return cities ?? new List<string>();
    }

    // delete
    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/business/{id}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't delete this business. Please try again."));
    }
    
    public async Task<BusinessDetailsModel?> GetOneById(int id)
    {
        var business = await _http.GetFromJsonAsync<BusinessDetailsModel>($"api/business/{id}");

        return business;
    }

    public async Task<(bool Success, string? Error)> AddPackageToBusiness(int businessId, PackageAddModel package)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync($"api/business/{businessId}/addPackage", package);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't add this package. Please try again."));
    }

    // ia lista de tipuri de pachet (pentru dropdown-ul din formular)
    public async Task<List<PackageTypeModel>> GetPackageTypesAsync()
    {
        var types = await _http.GetFromJsonAsync<List<PackageTypeModel>>("api/packagetype");
        return types ?? new List<PackageTypeModel>();
    }

    // get one package (for the edit form)
    public async Task<PackageModel?> GetPackageByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<PackageModel>($"api/package/{id}");
    }

    // update a package
    public async Task<(bool Success, string? Error)> UpdatePackageAsync(int id, PackageAddModel package)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/package/{id}", package);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't update this package. Please try again."));
    }

    // delete a package
    public async Task<(bool Success, string? Error)> DeletePackageAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.DeleteAsync($"api/package/{id}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't delete this package. Please try again."));
    }

    // business types for the dropdown
    public async Task<List<BusinessTypeModel>> GetBusinessTypesAsync()
    {
        var types = await _http.GetFromJsonAsync<List<BusinessTypeModel>>("api/businesstype");
        return types ?? new List<BusinessTypeModel>();
    }

    // business data for the edit form
    public async Task<BusinessAddModel?> GetBusinessForEditAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.GetAsync($"api/business/{id}/edit");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<BusinessAddModel>();
    }

    // create a business
    public async Task<(bool Success, string? Error)> AddBusinessAsync(BusinessAddModel business)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PostAsJsonAsync("api/business", business);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't save this business. Please try again."));
    }

    // update a business
    public async Task<(bool Success, string? Error)> UpdateBusinessAsync(int id, BusinessAddModel business)
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.PutAsJsonAsync($"api/business/{id}", business);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't save this business. Please try again."));
    }

    public async Task<List<BusinessDetailsModel>> GetMyBusinessesAsync()
    {
        await EnsureAuthHeaderAsync();
        var response = await _http.GetAsync("api/business/mine");
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<BusinessDetailsModel>>() ?? new();
    }

    public async Task<(string? Url, string? Error)> UploadImageAsync(IBrowserFile file)
    {
        await EnsureAuthHeaderAsync();
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(file.OpenReadStream(5 * 1024 * 1024));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _http.PostAsync("api/upload", content);
        if (!response.IsSuccessStatusCode)
            return (null, await ApiError.ReadAsync(response, "Image upload failed. Use a jpg, png or webp under 5 MB."));

        var result = await response.Content.ReadFromJsonAsync<UploadResultModel>();
        return (result?.Url, null);
    }
}
