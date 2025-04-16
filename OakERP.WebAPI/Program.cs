using OakERP.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register all services (modular)
builder.Services.AddApplicationDb(builder.Configuration).AddIdentityServices().AddSwaggerDocs();

var app = builder.Build();

// Use middleware and tools (modular)
app.UseOakMiddleware();

app.Run();