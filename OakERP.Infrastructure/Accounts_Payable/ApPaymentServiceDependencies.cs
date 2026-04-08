using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Accounts_Payable;

public sealed class ApPaymentServiceDependencies(
    IGlSettingsProvider glSettingsProvider,
    IUnitOfWork unitOfWork
)
{
    public IGlSettingsProvider GlSettingsProvider { get; } = glSettingsProvider;

    public IUnitOfWork UnitOfWork { get; } = unitOfWork;
}
