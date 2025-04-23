using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OakERP.Auth;
using OakERP.Domain.Entities;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Tests.Integration.TestSetup;

public static class AuthTestFactory
{
    public static IAuthService Create(
        ApplicationDbContext dbContext,
        IConfiguration? configOverride = null
    )
    {
        var userStore = new UserStore<ApplicationUser>(dbContext);

        var userManager = new UserManager<ApplicationUser>(
            userStore,
            new OptionsWrapper<IdentityOptions>(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new LoggerFactory().CreateLogger<UserManager<ApplicationUser>>()
        );

        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider(),
        };

        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

        var userClaimsPrincipalFactory = new UserClaimsPrincipalFactory<ApplicationUser>(
            userManager,
            new OptionsWrapper<IdentityOptions>(new IdentityOptions())
        );

        var signInManager = new SignInManager<ApplicationUser>(
            userManager,
            httpContextAccessor,
            userClaimsPrincipalFactory,
            null,
            null,
            null,
            null
        );

        var config =
            configOverride
            ?? new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        var jwtGenerator = new JwtGenerator(config);

        return new AuthService(userManager, signInManager, dbContext, config, jwtGenerator);
    }
}