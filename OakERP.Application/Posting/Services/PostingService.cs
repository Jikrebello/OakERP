using OakERP.Common.Enums;

namespace OakERP.Application.Posting.Services;

public sealed class PostingService : IPostingService
{
    private readonly ApPaymentPostingOperation apPaymentOperation;
    private readonly ApInvoicePostingOperation apInvoiceOperation;
    private readonly ArInvoicePostingOperation arInvoiceOperation;
    private readonly ArReceiptPostingOperation arReceiptOperation;

    public PostingService(
        PostingSourceRepositories sourceRepositories,
        PostingPersistenceDependencies persistenceDependencies,
        PostingRuntimeDependencies runtimeDependencies,
        PostingContextBuilders contextBuilders
    )
    {
        var resultProcessor = new PostingResultProcessor(persistenceDependencies);
        var transactionExecutor = new PostingTransactionExecutor(persistenceDependencies);
        var support = new PostingOperationSupport(
            persistenceDependencies,
            runtimeDependencies,
            resultProcessor
        );

        apPaymentOperation = new ApPaymentPostingOperation(
            sourceRepositories.ApPaymentRepository,
            contextBuilders.ApPaymentPostingContextBuilder,
            support,
            transactionExecutor
        );
        apInvoiceOperation = new ApInvoicePostingOperation(
            sourceRepositories.ApInvoiceRepository,
            contextBuilders.ApInvoicePostingContextBuilder,
            support,
            transactionExecutor
        );
        arInvoiceOperation = new ArInvoicePostingOperation(
            sourceRepositories.ArInvoiceRepository,
            contextBuilders.ArInvoicePostingContextBuilder,
            support,
            transactionExecutor
        );
        arReceiptOperation = new ArReceiptPostingOperation(
            sourceRepositories.ArReceiptRepository,
            contextBuilders.ArReceiptPostingContextBuilder,
            support,
            transactionExecutor
        );
    }

    public Task<PostResult> PostAsync(
        PostCommand command,
        CancellationToken cancellationToken = default
    ) =>
        command.DocKind switch
        {
            DocKind.ApPayment => apPaymentOperation.PostAsync(command, cancellationToken),
            DocKind.ApInvoice => apInvoiceOperation.PostAsync(command, cancellationToken),
            DocKind.ArInvoice => arInvoiceOperation.PostAsync(command, cancellationToken),
            DocKind.ArReceipt => arReceiptOperation.PostAsync(command, cancellationToken),
            _ => throw new NotSupportedException(
                $"Posting for document kind '{command.DocKind}' is not supported."
            ),
        };

    public Task<UnpostResult> UnpostAsync(
        UnpostCommand command,
        CancellationToken cancellationToken = default
    ) => throw new NotSupportedException("Unposting is not supported for posting.");
}
