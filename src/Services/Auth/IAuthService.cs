using BlazorApp.Model;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.Services.Auth;

public interface IAuthService
{
    Task<LoginResult> Login(LoginModel loginModel);
    Task Logout();
    Task<RegisterResult> Register(RegisterModel registerModel);
    Task<AuthenticationState> GetAuthenticationStateAsync();
}