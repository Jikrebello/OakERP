using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Application.AccountsReceivable.Receipts.Support;

public sealed class ArReceiptServiceDependencies(
    IGlSettingsProvider glSettingsProvider,
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier persistenceFailureClassifier,
    IClock clock
)
{
    public IGlSettingsProvider GlSettingsProvider { get; } = glSettingsProvider;

    public IUnitOfWork UnitOfWork { get; } = unitOfWork;

    public IPersistenceFailureClassifier PersistenceFailureClassifier { get; } =
        persistenceFailureClassifier;

    public IClock Clock { get; } = clock;
}
