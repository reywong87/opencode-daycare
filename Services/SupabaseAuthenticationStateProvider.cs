using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using OpenDaycare.Models;

namespace OpenDaycare.Services;

public sealed class SupabaseAuthenticationStateProvider(SupabaseAuthService authService) : AuthenticationStateProvider
{
    private AuthenticationState authenticationState = Anonymous;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(authenticationState);

    public async Task<bool> RefreshAuthenticationStateAsync()
    {
        var authenticatedUser = await authService.GetAuthenticatedUserAsync();
        if (authenticatedUser is null || !string.Equals(authenticatedUser.Status, "active", StringComparison.Ordinal))
        {
            await SignOutAsync();
            return false;
        }

        SetAuthenticationState(new AuthenticationState(CreatePrincipal(authenticatedUser)));
        return true;
    }

    public async Task SignOutAsync()
    {
        try
        {
            await authService.SignOutAsync();
        }
        finally
        {
            SetAuthenticationState(Anonymous);
        }
    }

    private void SetAuthenticationState(AuthenticationState newAuthenticationState)
    {
        authenticationState = newAuthenticationState;
        NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
    }

    private static ClaimsPrincipal CreatePrincipal(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        if (!string.IsNullOrWhiteSpace(user.DaycareName))
        {
            claims.Add(new Claim("daycare_name", user.DaycareName));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Supabase"));
    }

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
}
