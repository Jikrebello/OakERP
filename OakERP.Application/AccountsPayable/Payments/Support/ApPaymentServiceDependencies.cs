using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Application.AccountsPayable.Payments.Support;

public sealed class ApPaymentServiceDependencies(
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
