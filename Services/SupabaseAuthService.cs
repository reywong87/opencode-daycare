using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;
using Supabase.Gotrue;

namespace OpenDaycare.Services;

public sealed class SupabaseAuthService(
    Supabase.Client supabase,
    IDataProtectionProvider dataProtection,
    IJSRuntime jsRuntime)
{
    private const string SessionStorageKey = "opendaycare.auth.session.v1";
    private readonly IDataProtector sessionProtector = dataProtection.CreateProtector("OpenDaycare.Supabase.Session.v1");
    private Task? initializeTask;

    public Task InitializeAsync() => initializeTask ??= supabase.InitializeAsync();

    public async Task<bool> SignInAsync(string email, string password)
    {
        await InitializeAsync();

        var session = await supabase.Auth.SignIn(email, password);
        if (session is null)
        {
            return false;
        }

        await PersistSessionAsync(session);
        return true;
    }

    public async Task<Session?> RestoreSessionAsync()
    {
        await InitializeAsync();

        var protectedSession = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", SessionStorageKey);
        if (string.IsNullOrWhiteSpace(protectedSession))
        {
            return null;
        }

        try
        {
            var storedSession = JsonSerializer.Deserialize<StoredSession>(sessionProtector.Unprotect(protectedSession));
            if (storedSession is null || string.IsNullOrWhiteSpace(storedSession.AccessToken) || string.IsNullOrWhiteSpace(storedSession.RefreshToken))
            {
                await ClearPersistedSessionAsync();
                return null;
            }

            var session = await supabase.Auth.SetSession(storedSession.AccessToken, storedSession.RefreshToken, false);
            if (session is null)
            {
                await ClearPersistedSessionAsync();
                return null;
            }

            await PersistSessionAsync(session);
            return session;
        }
        catch
        {
            await ClearPersistedSessionAsync();
            return null;
        }
    }

    public async Task SignOutAsync()
    {
        await InitializeAsync();

        try
        {
            await supabase.Auth.SignOut();
        }
        finally
        {
            await ClearPersistedSessionAsync();
        }
    }

    private async Task PersistSessionAsync(Session session)
    {
        if (string.IsNullOrWhiteSpace(session.AccessToken) || string.IsNullOrWhiteSpace(session.RefreshToken))
        {
            throw new InvalidOperationException("Supabase returned a session without tokens.");
        }

        var serializedSession = JsonSerializer.Serialize(new StoredSession(session.AccessToken, session.RefreshToken));
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", SessionStorageKey, sessionProtector.Protect(serializedSession));
    }

    private Task ClearPersistedSessionAsync() => jsRuntime.InvokeVoidAsync("localStorage.removeItem", SessionStorageKey).AsTask();

    private sealed record StoredSession(string AccessToken, string RefreshToken);
}
