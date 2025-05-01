using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Providers;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    private ClaimsPrincipal _user;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_user ?? _anonymous));
    }

    public void MarkUserAsAuthenticated(string username)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin")
        }, "apiauth_type");

        _user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }

    public void MarkUserAsLoggedOut()
    {
        _user = _anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
