using OakERP.Application.Interfaces.Persistence;

namespace OakERP.Application.Common.Orchestration;

public sealed class InvoiceCreateWorkflowDependencies(
    IUnitOfWork unitOfWork,
    IPersistenceFailureClassifier persistenceFailureClassifier,
    IClock clock
)
{
    public IPersistenceFailureClassifier PersistenceFailureClassifier { get; } =
        persistenceFailureClassifier;

    public IClock Clock { get; } = clock;

    internal ResultWorkflowTransactionRunner TransactionRunner { get; } = new(unitOfWork);
}
