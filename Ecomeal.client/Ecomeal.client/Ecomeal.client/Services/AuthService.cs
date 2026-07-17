using Ecomeal.client.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Ecomeal.client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public string? Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    public List<string> Roles { get; private set; } = new();
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsPartner => Roles.Contains("Partner");
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? BusinessName { get; private set; }
    public string? PreferredPackageTypes { get; private set; }

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
                Country = me?.Country;
                BusinessName = me?.BusinessName;
                PreferredPackageTypes = me?.PreferredPackageTypes;
                await _localStorage.SetAsync("userRoles", Roles);
                await _localStorage.SetAsync("userCity", City ?? "");
                await _localStorage.SetAsync("userBusinessName", BusinessName ?? "");

                if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                    customProvider.NotifyUserAuthentication(Token, Roles);
            }
            return AuthResult.Ok();
        }

        var body = await response.Content.ReadAsStringAsync();
        if (body.Contains("NotAllowed"))
            return new AuthResult
            {
                Success = false,
                NeedsEmailConfirmation = true,
                ErrorMessage = "Your email is not verified yet. Enter the code we sent you to activate your account."
            };

        return AuthResult.Fail("Invalid email or password.");
    }

    public async Task<(bool Success, string? Error)> VerifyEmailAsync(string email, string code)
    {
        var response = await _http.PostAsJsonAsync("api/auth/verify-email", new { Email = email, Code = code });
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't verify your email. Please try again."));
    }

    public async Task<(bool Success, string? Error)> ResendVerificationCodeAsync(string email)
    {
        var response = await _http.PostAsJsonAsync("api/auth/resend-code", new { Email = email });
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't resend the code. Please try again."));
    }

    public async Task LoadTokenAsync()
    {
        try
        {
            var tokenResult = await _localStorage.GetAsync<string>("authToken");
            Token = tokenResult.Success ? tokenResult.Value : null;
        }
        catch
        {
            await LogoutAsync();
            return;
        }

        if (Token != null)
        {
            var me = await FetchMeAsync(Token);
            if (me is null)
            {
                await LogoutAsync();
                return;
            }

            Roles = me.Roles ?? new List<string>();
            City = me.City;
            Country = me.Country;
            BusinessName = me.BusinessName;
            PreferredPackageTypes = me.PreferredPackageTypes;
            await _localStorage.SetAsync("userRoles", Roles);
            await _localStorage.SetAsync("userCity", City ?? "");
            await _localStorage.SetAsync("userBusinessName", BusinessName ?? "");

            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                customProvider.NotifyUserAuthentication(Token, Roles);
        }
    }

    public async Task LogoutAsync()
    {
        Token = null;
        Roles = new();
        City = null;
        Country = null;
        BusinessName = null;
        try
        {
            await _localStorage.DeleteAsync("authToken");
            await _localStorage.DeleteAsync("userRoles");
            await _localStorage.DeleteAsync("userCity");
            await _localStorage.DeleteAsync("userBusinessName");
        }
        catch
        {
        }

        if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            customProvider.NotifyUserLogout();
    }

    public async Task<UserMeResponse?> GetMeAsync()
    {
        if (Token == null)
            await LoadTokenAsync();

        return Token == null ? null : await FetchMeAsync(Token);
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(string? name, string? contact)
    {
        if (Token == null)
            return (false, "You are not logged in.");

        var req = new HttpRequestMessage(HttpMethod.Put, "api/auth/profile")
        {
            Content = JsonContent.Create(new { Name = name, Contact = contact })
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        var response = await _http.SendAsync(req);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiError.ReadAsync(response, "We couldn't update your profile. Please try again."));
    }

    public async Task<AuthResult> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        if (Token == null)
            return AuthResult.Fail("You are not logged in.");

        var req = new HttpRequestMessage(HttpMethod.Put, "api/auth/password")
        {
            Content = JsonContent.Create(new { CurrentPassword = currentPassword, NewPassword = newPassword })
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        var response = await _http.SendAsync(req);
        if (response.IsSuccessStatusCode)
            return AuthResult.Ok();

        var error = await response.Content.ReadFromJsonAsync<RegisterErrorResponse>();
        var errorMessage = error?.Errors != null ? string.Join("; ", error.Errors) : "Changing the password failed.";
        return AuthResult.Fail(errorMessage);
    }

    public async Task<(bool Success, string? Error)> UpdateCityAsync(string city)
    {
        if (Token == null)
            return (false, "You are not logged in.");

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
            return (true, null);
        }

        return (false, await ApiError.ReadAsync(response, "We couldn't update your city. Please try again."));
    }

    public async Task<List<AdminUserModel>> GetUsersAsync()
    {
        if (Token == null)
            await LoadTokenAsync();
        if (Token == null)
            return new();

        var req = new HttpRequestMessage(HttpMethod.Get, "api/auth/users");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return new();

        return await response.Content.ReadFromJsonAsync<List<AdminUserModel>>() ?? new();
    }

    public async Task<(bool Success, string? Error)> UpdatePreferencesAsync(string? preferredPackageTypes)
    {
        if (Token == null)
            return (false, "You are not logged in.");

        var req = new HttpRequestMessage(HttpMethod.Put, "api/auth/preferences")
        {
            Content = JsonContent.Create(new { PreferredPackageTypes = preferredPackageTypes })
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        var response = await _http.SendAsync(req);
        if (response.IsSuccessStatusCode)
        {
            PreferredPackageTypes = preferredPackageTypes;
            return (true, null);
        }

        return (false, await ApiError.ReadAsync(response, "We couldn't save your preferences. Please try again."));
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
