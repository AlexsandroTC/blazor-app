using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
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

            var claims = ParseClaimsFromJwt(token);
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

        var claims = ParseClaimsFromJwt(token);
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;

        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles))
        {
            if (roles.ToString().Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                foreach (var parsedRole in parsedRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
            }
            keyValuePairs.Remove(ClaimTypes.Role);
        }

        claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
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