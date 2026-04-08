using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptServiceDependencies(
    IGlSettingsProvider glSettingsProvider,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier persistenceFailureClassifier
)
{
    public IGlSettingsProvider GlSettingsProvider { get; } = glSettingsProvider;

    public IUnitOfWork UnitOfWork { get; } = unitOfWork;

    public IPersistenceFailureClassifier PersistenceFailureClassifier { get; } =
        persistenceFailureClassifier;
}
