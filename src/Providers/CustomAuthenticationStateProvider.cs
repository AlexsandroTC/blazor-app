using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using BlazorApp.Helper;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Providers;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    private ClaimsPrincipal _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedUser.Identity != null && _cachedUser.Identity.IsAuthenticated)
            return new AuthenticationState(_cachedUser);

        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            var claims = JwtHelper.ParseClaimsFromJwt(token);
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            return new AuthenticationState(_cachedUser);
        }
        catch (InvalidOperationException)
        {
            // Ainda não é possível acessar o JSInterop (prerender)
            // Retorna um usuário vazio para não quebrar a UI
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

        var claims = JwtHelper.ParseClaimsFromJwt(token);
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }

    public void SetUser(ClaimsPrincipal user)
    {
        _cachedUser = user;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }
    
    public void ForceNotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }
}