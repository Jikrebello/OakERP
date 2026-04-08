using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Client;
using OakERP.Client.Extensions;
using OakERP.Client.Services.Api;
using OakERP.Common.Abstractions;
using OakERP.Shared.Services;
using OakERP.UI.Extensions;
using OakERP.Web.Components;
using OakERP.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var apiOptions = new ApiClientOptions
{
    BaseUrl =
        builder.Configuration["Api:BaseUrl"]
        ?? Environment.GetEnvironmentVariable("OakERP__Api__BaseUrl")
        ?? throw new InvalidOperationException("Api:BaseUrl is not configured."),
};

// Add Razor Components & Fluent UI
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// Add device-specific services used by OakERP.Shared
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ITokenStore, BlazorTokenStore>();
builder.Services.AddScoped<IPlatformService, BlazorPlatformService>();

// Register shared Razor Class Library services
builder.Services.AddOakClientCoreServices();
builder.Services.AddOakAuthUiState();
builder.Services.AddOakApiClient(apiOptions);

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
