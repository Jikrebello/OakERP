using Microsoft.AspNetCore.Components;

namespace OakERP.Shared.Extensions;

public static class NavigationExtensions
{
    public static bool IsOnPublicRoute(this NavigationManager nav, params string[] publicRoutes)
    {
        var relativePath = nav.ToBaseRelativePath(nav.Uri).ToLowerInvariant();
        return publicRoutes.Any(r =>
            relativePath.Equals(r, StringComparison.InvariantCultureIgnoreCase)
        );
    }
}
