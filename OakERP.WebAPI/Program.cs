using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities;
using OakERP.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with Postgres
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configure Identity (with ApplicationUser + EF store)
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Optional: configure password strength etc.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});

builder.Services.AddOpenApi(); // your OpenAPI/Swagger setup

var app = builder.Build();

// Swagger UI only in dev
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
