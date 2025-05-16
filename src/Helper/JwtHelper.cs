using System.Security.Claims;
using System.Text.Json;

namespace BlazorApp.Helper;

public static class JwtHelper
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        // Verificação adicional de segurança
        if (string.IsNullOrWhiteSpace(jwt) || jwt.Count(c => c == '.') != 2)
        {
            return claims;
        }

        try
        {
            var parts = jwt.Split('.');
            var payload = parts[1];
            var jsonBytes = ParseBase64Url(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            // Processamento de roles
            if (keyValuePairs.TryGetValue("role", out object roles))
            {
                ProcessRoles(claims, roles);
                keyValuePairs.Remove("role");
            }

            // Adiciona todas as outras claims
            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

            // Garante ClaimTypes.Name
            EnsureNameClaim(claims, keyValuePairs);

            return claims;
        }
        catch
        {
            // Em caso de erro, retorna lista vazia
            return new List<Claim>();
        }
    }

    private static void ProcessRoles(List<Claim> claims, object roles)
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
    }

    private static void EnsureNameClaim(List<Claim> claims, Dictionary<string, object> keyValuePairs)
    {
        if (!claims.Any(c => c.Type == ClaimTypes.Name))
        {
            if (keyValuePairs.TryGetValue("name", out var name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()));
            }
            else if (keyValuePairs.TryGetValue("unique_name", out var uniqueName))
            {
                claims.Add(new Claim(ClaimTypes.Name, uniqueName.ToString()));
            }
            else if (keyValuePairs.TryGetValue("sub", out var sub))
            {
                claims.Add(new Claim(ClaimTypes.Name, sub.ToString()));
            }
        }
    }

    private static byte[] ParseBase64Url(string base64Url)
    {
        // Remove todos os caracteres especiais
        var cleaned = base64Url.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "");
        
        // Substitui caracteres URL-safe
        var base64 = cleaned.Replace('-', '+').Replace('_', '/');
        
        // Calcula padding necessário
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}