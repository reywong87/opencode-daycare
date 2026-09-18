using OpenDaycare.Components;
using OpenDaycare.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDataProtection();
builder.Services.AddScoped<ChildrenService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<SupabaseAuthService>();
builder.Services.AddScoped<SupabaseAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(serviceProvider => serviceProvider.GetRequiredService<SupabaseAuthenticationStateProvider>());

var supabaseUrl = builder.Configuration["Supabase:Url"]
                  ?? throw new InvalidOperationException("Missing Supabase:Url.");
var supabaseKey = builder.Configuration["Supabase:AnonKey"]
                  ?? throw new InvalidOperationException("Missing Supabase:AnonKey.");

builder.Services.AddScoped(_ =>
{
    var client = new Supabase.Client(
        supabaseUrl,
        supabaseKey,
        new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = false
        });
    client.Storage.GetHeaders = () => new Dictionary<string, string>
    {
        ["apikey"] = supabaseKey,
        ["Authorization"] = $"Bearer {client.Auth.CurrentSession?.AccessToken ?? supabaseKey}"
    };
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
