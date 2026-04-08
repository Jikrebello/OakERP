using Microsoft.Extensions.DependencyInjection;
using OakERP.Application.Settlements.Documents;

namespace OakERP.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<SettlementDocumentWorkflowDependencies>();
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
