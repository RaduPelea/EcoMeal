using EcoMeal.client.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace EcoMeal.client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public string? Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    public List<string> Roles { get; private set; } = new();
    public bool IsAdmin => Roles.Contains("Admin");
    public string? City { get; private set; }

    public AuthService(HttpClient http, ProtectedLocalStorage localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string name, string contact, string city)
    {
        var request = new RegisterRequest { Email = email, Password = password, Name = name, Contact = contact, City = city };
        var response = await _http.PostAsJsonAsync("api/auth/register", request);

        if (response.IsSuccessStatusCode)
            return AuthResult.Ok();

        var error = await response.Content.ReadFromJsonAsync<RegisterErrorResponse>();
        var errorMessage = error?.Errors != null ? string.Join("; ", error.Errors) : "Registration failed.";
        return AuthResult.Fail(errorMessage);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var request = new AuthRequest { Email = email, Password = password };
        var response = await _http.PostAsJsonAsync("login", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Token = result?.AccessToken;

            if (Token != null)
            {
                await _localStorage.SetAsync("authToken", Token);

                var me = await FetchMeAsync(Token);
                Roles = me?.Roles ?? new List<string>();
                City = me?.City;
                await _localStorage.SetAsync("userRoles", Roles);
                await _localStorage.SetAsync("userCity", City ?? "");

                if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                    customProvider.NotifyUserAuthentication(Token, Roles);
            }
            return AuthResult.Ok();
        }
        return AuthResult.Fail("Invalid email or password.");
    }

    public async Task LoadTokenAsync()
    {
        var tokenResult = await _localStorage.GetAsync<string>("authToken");
        Token = tokenResult.Success ? tokenResult.Value : null;

        if (Token != null)
        {
            var rolesResult = await _localStorage.GetAsync<List<string>>("userRoles");
            Roles = rolesResult.Success && rolesResult.Value != null ? rolesResult.Value : new List<string>();

            var cityResult = await _localStorage.GetAsync<string>("userCity");
            City = cityResult.Success ? cityResult.Value : null;

            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                customProvider.NotifyUserAuthentication(Token, Roles);
        }
    }

    public async Task LogoutAsync()
    {
        Token = null;
        Roles = new();
        City = null;
        await _localStorage.DeleteAsync("authToken");
        await _localStorage.DeleteAsync("userRoles");
        await _localStorage.DeleteAsync("userCity");

        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            customProvider.NotifyUserLogout();
    }

    public async Task<bool> UpdateCityAsync(string city)
    {
        if (Token == null) return false;

        var req = new HttpRequestMessage(HttpMethod.Put, "api/auth/city")
        {
            Content = JsonContent.Create(new { City = city })
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        var response = await _http.SendAsync(req);
        if (response.IsSuccessStatusCode)
        {
            City = city;
            await _localStorage.SetAsync("userCity", city);
            return true;
        }
        return false;
    }

    private async Task<UserMeResponse?> FetchMeAsync(string token)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(req);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UserMeResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching me: {ex.Message}");
        }
        return null;
    }
}
