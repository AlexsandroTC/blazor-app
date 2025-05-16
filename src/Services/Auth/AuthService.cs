using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BlazorApp.Model;
using BlazorApp.Providers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;
    public AuthService(HttpClient httpClient,
                       AuthenticationStateProvider authenticationStateProvider,
                       ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
    }
    public async Task<LoginResult> Login(LoginModel loginModel)
    {
        // Todo: Adicionar a chamada com a API.
        // var loginAsJson = JsonSerializer.Serialize(loginModel);
        // var response = await _httpClient.PostAsync("api/Login",new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
        // var loginResult = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // if (!response.IsSuccessStatusCode)
        // {
        //     return loginResult;
        // }
        
        var loginResult = new LoginResult();
        loginResult.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOlsiQWRtaW4iLCJVc2VyIl0sIm5hbWUiOiJBbGV4c2FuZHJvIiwiZW1haWwiOiJhbGV4QGV4YW1wbGUuY29tIn0.signature";
        loginResult.Successful = true;
        
        await _localStorage.SetItemAsync("authToken", loginResult.Token);
        Console.WriteLine($"[DEBUG] Token before header: '{loginResult.Token}'");
        
        await ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginResult.Token);
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token?.Trim().Replace("\n", "").Replace("\r", ""));
        return loginResult;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<RegisterResult> Register(RegisterModel registerModel)
    {
        var messageResult = await _httpClient.PostAsJsonAsync("api/account", registerModel);
        var result = await messageResult.Content.ReadFromJsonAsync<RegisterResult>();
        return result;
    }
}