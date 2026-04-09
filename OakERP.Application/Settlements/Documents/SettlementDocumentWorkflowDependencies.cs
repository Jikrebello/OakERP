using OakERP.Application.Common.Orchestration;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Domain.Posting.GeneralLedger;

namespace OakERP.Application.Settlements.Documents;

public sealed class SettlementDocumentWorkflowDependencies(
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

    internal SettlementDocumentCreateWorkflowRunner CreateWorkflowRunner { get; } =
        new(new ResultWorkflowTransactionRunner(unitOfWork));

    internal SettlementDocumentAllocateWorkflowRunner AllocateWorkflowRunner { get; } =
        new(new ResultWorkflowTransactionRunner(unitOfWork));
}
