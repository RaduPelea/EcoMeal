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

    // get one package (for the edit form)
    public async Task<PackageModel?> GetPackageByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<PackageModel>($"api/package/{id}");
    }

    // update a package
    public async Task UpdatePackageAsync(int id, PackageAddModel package)
    {
        await _http.PutAsJsonAsync($"api/package/{id}", package);
    }

    // delete a package
    public async Task<bool> DeletePackageAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/package/{id}");
        return response.IsSuccessStatusCode;
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
        return await _http.GetFromJsonAsync<BusinessAddModel>($"api/business/{id}/edit");
    }

    // create a business
    public async Task AddBusinessAsync(BusinessAddModel business)
    {
        await _http.PostAsJsonAsync("api/business", business);
    }

    // update a business
    public async Task UpdateBusinessAsync(int id, BusinessAddModel business)
    {
        await _http.PutAsJsonAsync($"api/business/{id}", business);
    }
}
