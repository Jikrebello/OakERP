using Microsoft.Extensions.DependencyInjection;

namespace OakERP.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ApPaymentServiceDependencies>();
        services.AddScoped<ArReceiptServiceDependencies>();
        services.AddScoped<PostingSourceRepositories>();
        services.AddScoped<PostingPersistenceDependencies>();
        services.AddScoped<PostingRuntimeDependencies>();
        services.AddScoped<PostingContextBuilders>();

        services.AddScoped<IApInvoiceService, ApInvoiceService>();
        services.AddScoped<IApPaymentService, ApPaymentService>();
        services.AddScoped<IArReceiptService, ArReceiptService>();
        services.AddScoped<IPostingService, PostingService>();

        return services;
    }
}
