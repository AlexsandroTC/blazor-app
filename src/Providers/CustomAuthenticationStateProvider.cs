using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Providers;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Em um cenário real, você pegaria o token JWT de um armazenamento local ou cookie
        // var jwt = await storage.GetItemAsync("AccessToken");

        // Aqui, você está criando um ClaimsPrincipal manualmente
        var identity = new ClaimsIdentity();

        identity.AddClaim(new Claim(ClaimTypes.Name, "Alexsandro"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void MarkUserAsAuthenticated(string username)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        }, "Fake");

        var user = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(user);

        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = new AuthenticationState(anonymous);

        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }
}
