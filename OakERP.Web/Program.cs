using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Common.Abstractions;
using OakERP.Shared.Extensions;
using OakERP.Shared.Services;
using OakERP.Shared.Services.Api;
using OakERP.Web.Components;
using OakERP.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var apiBaseUrl =
    builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

// Add Razor Components & Fluent UI
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// Add device-specific services used by OakERP.Shared
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ITokenStore, BlazorTokenStore>();
builder.Services.AddScoped<IPlatformService, BlazorPlatformService>();

builder.Services.AddScoped<AuthTokenHandler>();

builder
    .Services.AddHttpClient<IApiClient, ApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<AuthTokenHandler>();

// Register shared Razor Class Library services
builder.Services.AddOakClientServices();

var app = builder.Build();

// HTTP pipeline config
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(OakERP.Shared._Imports).Assembly);

app.Run();
