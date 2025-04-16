using OakERP.UI;

var builder = WebApplication.CreateBuilder(args);

// Add Razor component services
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

// Error handling & HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAntiforgery();

// Map the shared App.razor from OakERP.UI
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();