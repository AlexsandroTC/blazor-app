namespace BlazorApp.Model;

public class LoginResult
{
    public string? Token { get; set; }
    public bool Successful { get; set; }
    public string Error { get; set; }
}