using OakERP.Common.Enums;
using OakERP.Domain.AccountsPayable;
using OakERP.Domain.Entities.AccountsPayable;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.AccountsPayable;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.AccountsPayable;
using OakERP.Domain.Entities.GeneralLedger;

namespace OakERP.Application.Posting.Operations;

internal sealed class ApPaymentPostingOperation(
    IApPaymentRepository apPaymentRepository,
    IApPaymentPostingContextBuilder contextBuilder,
    PostingOperationSupport support,
    PostingTransactionExecutor transactionExecutor
)
{
    public Task<PostResult> PostAsync(PostCommand command, CancellationToken cancellationToken)
    {
        PostingOperationSupport.EnsureForceDisabled(
            command.Force,
            "Force posting is not supported for AP payment posting."
        );

        return transactionExecutor.ExecuteAsync(
            async ct =>
            {
                ApPayment payment =
                    await apPaymentRepository.GetTrackedForPostingAsync(command.SourceId, ct)
                    ?? throw new InvalidOperationException("AP payment was not found.");

                PostingOperationSupport.EnsureDraftStatus(
                    payment.DocStatus,
                    "Only draft AP payments can be posted."
                );

                GlPostingSettings settings = await support.GetSettingsAsync(ct);
                DateOnly postingDate = command.PostingDate ?? payment.PaymentDate;

                if (payment.BankAccount is null)
                {
                    throw new InvalidOperationException("AP payment bank account was not found.");
                }

                if (!payment.BankAccount.IsActive)
                {
                    throw new InvalidOperationException(
                        "AP payment posting requires an active bank account."
                    );
                }

                PostingOperationSupport.EnsureBaseCurrency(
                    payment.BankAccount.CurrencyCode,
                    settings.BaseCurrencyCode,
                    "AP payment posting currently supports only payments in the base currency."
                );

                decimal allocatedAmount = ApSettlementCalculator.GetPaymentAllocatedAmount(payment);
                if (allocatedAmount > payment.Amount)
                {
                    throw new InvalidOperationException(
                        "AP payment allocations exceed the payment amount and cannot be posted."
                    );
                }

                FiscalPeriod period = await support.GetOpenPeriodAsync(postingDate, ct);
                PostingRule rule = await support.GetActiveRuleAsync(DocKind.ApPayment, ct);

                ApPaymentPostingContext context = await contextBuilder.BuildAsync(
                    payment,
                    postingDate,
                    period,
                    settings,
                    rule,
                    ct
                );

                PostingEngineResult postingResult = support.PostingEngine.PostApPayment(context);
                await support.ProcessPostingResultAsync(
                    postingResult,
                    PostingSourceTypes.ApPayment,
                    inventoryRowsAllowed: false,
                    command.PerformedBy,
                    ct
                );

                payment.DocStatus = DocStatus.Posted;
                payment.PostingDate = postingDate;
                payment.UpdatedBy = command.PerformedBy;
                payment.UpdatedAt = DateTimeOffset.UtcNow;

                return new PostResult(
                    command.DocKind,
                    payment.Id,
                    payment.DocNo,
                    postingDate,
                    period.Id,
                    postingResult.GlEntries.Count,
                    postingResult.InventoryMovements.Count
                );
            },
            "The AP payment was modified during posting. It may already be posted.",
            cancellationToken
        );
    }
}
