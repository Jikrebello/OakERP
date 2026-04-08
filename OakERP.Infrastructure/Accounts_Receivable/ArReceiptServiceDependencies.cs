using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Accounts_Receivable;

public sealed class ArReceiptServiceDependencies(
    IGlSettingsProvider glSettingsProvider,
    IUnitOfWork unitOfWork
)
{
    public IGlSettingsProvider GlSettingsProvider { get; } = glSettingsProvider;

    public IUnitOfWork UnitOfWork { get; } = unitOfWork;
}
