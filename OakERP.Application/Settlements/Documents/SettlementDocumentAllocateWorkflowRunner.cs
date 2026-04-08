using OakERP.Application.Common.Orchestration;
using OakERP.Common.Dtos.Base;

namespace OakERP.Application.Settlements.Documents;

internal sealed class SettlementDocumentAllocateWorkflowRunner(
    ResultWorkflowTransactionRunner transactionRunner
)
{
    public Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken
    )
        where TResult : BaseResultDto =>
        transactionRunner.ExecuteAsync(operation, cancellationToken);
}
