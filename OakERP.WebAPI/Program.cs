using OakERP.Infrastructure.Persistence;
using OakERP.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register all services (modular)
builder
    .Services.AddApplicationDb(builder.Configuration)
    .AddIdentityServices()
    .AddJwtAuth(builder.Configuration)
    .AddAuthServices()
    .AddSwaggerDocs();

var app = builder.Build();

// Use middleware and tools (modular)
app.UseOakMiddleware();

await DbInitializer.SeedRolesAndAdminAsync(app.Services, app.Configuration);

app.Run();
