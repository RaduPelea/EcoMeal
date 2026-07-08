using Ecomeal.client.Models;

namespace Ecomeal.client.Services;

// serviciul care vorbeste utilizatorul cu api prin hhttp

public class BusinessService
{
    private readonly HttpClient _http;

    public BusinessService(HttpClient http)
    {
        _http = http;
    }

    // read: cere lista de afaceri de la api 
    public async Task<List<BusinessModel>> GetAllAsync()
    {
        var businesses = await _http.GetFromJsonAsync<List<BusinessModel>>("api/business");
        return businesses ?? new List<BusinessModel>();
    }

    // delete
    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/business/{id}");
        return response.IsSuccessStatusCode;
    }
    
    public async Task<BusinessDetailsModel?> GetOneById(int id)
    {
        var business = await _http.GetFromJsonAsync<BusinessDetailsModel>($"api/business/{id}");

        return business;
    }

    public async Task AddPackageToBusiness(int BusinessId,PackageAddModel package)
    {
        await _http.PostAsJsonAsync($"api/business/{BusinessId}/addPackage", package);
    }

    // ia lista de tipuri de pachet (pentru dropdown-ul din formular)
    public async Task<List<PackageTypeModel>> GetPackageTypesAsync()
    {
        var types = await _http.GetFromJsonAsync<List<PackageTypeModel>>("api/packagetype");
        return types ?? new List<PackageTypeModel>();
    }
}
