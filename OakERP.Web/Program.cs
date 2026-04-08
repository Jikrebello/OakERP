using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Client.Configuration;
using OakERP.Common.Abstractions;
using OakERP.Common.Exceptions;
using OakERP.Shared.Extensions;
using OakERP.Shared.Services;
using OakERP.Web.Components;
using OakERP.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.Local.json",
    optional: true,
    reloadOnChange: true
);

var apiBaseUrl =
    builder.Configuration["Api:BaseUrl"]
    ?? Environment.GetEnvironmentVariable("OakERP__Api__BaseUrl");

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new ConfigurationValidationException(
        "Api:BaseUrl",
        "Api:BaseUrl is not configured."
    );
}

var apiOptions = new ApiClientOptions
{
    BaseUrl = apiBaseUrl,
};

apiOptions.GetBaseUri();

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
builder.Services.AddOakSharedHostServices(apiOptions);

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
