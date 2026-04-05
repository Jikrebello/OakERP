using Microsoft.EntityFrameworkCore;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Inventory;

namespace OakERP.Infrastructure.Posting;

public sealed class PostingService(
    IArInvoiceRepository arInvoiceRepository,
    IArReceiptRepository arReceiptRepository,
    IFiscalPeriodRepository fiscalPeriodRepository,
    IGlAccountRepository glAccountRepository,
    IGlEntryRepository glEntryRepository,
    IInventoryLedgerRepository inventoryLedgerRepository,
    IGlSettingsProvider glSettingsProvider,
    IPostingRuleProvider postingRuleProvider,
    IArInvoicePostingContextBuilder arInvoicePostingContextBuilder,
    IArReceiptPostingContextBuilder arReceiptPostingContextBuilder,
    IPostingEngine postingEngine,
    IUnitOfWork unitOfWork
) : IPostingService
{
    public Task<PostResult> PostAsync(
        PostCommand command,
        CancellationToken cancellationToken = default
    ) =>
        command.DocKind switch
        {
            DocKind.ArInvoice => PostArInvoiceAsync(command, cancellationToken),
            DocKind.ArReceipt => PostArReceiptAsync(command, cancellationToken),
            _ => throw new NotSupportedException(
                $"Posting for document kind '{command.DocKind}' is not supported."
            ),
        };

    public Task<UnpostResult> UnpostAsync(
        UnpostCommand command,
        CancellationToken cancellationToken = default
    ) => throw new NotSupportedException("Unposting is not supported for posting.");

    private async Task<PostResult> PostArInvoiceAsync(
        PostCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.Force)
        {
            throw new InvalidOperationException(
                "Force posting is not supported for AR invoice posting."
            );
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            ArInvoice invoice =
                await arInvoiceRepository.GetTrackedForPostingAsync(
                    command.SourceId,
                    cancellationToken
                ) ?? throw new InvalidOperationException("AR invoice was not found.");

            if (invoice.DocStatus != DocStatus.Draft)
            {
                throw new InvalidOperationException("Only draft AR invoices can be posted.");
            }

            GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                cancellationToken
            );
            DateOnly postingDate = command.PostingDate ?? invoice.InvoiceDate;

            if (
                !ArSettlementCalculator.MatchesCurrency(
                    invoice.CurrencyCode,
                    settings.BaseCurrencyCode
                )
            )
            {
                throw new InvalidOperationException(
                    "AR invoice posting currently supports only invoices in the base currency."
                );
            }

            FiscalPeriod period =
                await fiscalPeriodRepository.GetOpenForDateAsync(postingDate, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"No open fiscal period exists for posting date {postingDate:yyyy-MM-dd}."
                );

            IReadOnlyList<ArInvoiceLine> lines = invoice.Lines.OrderBy(x => x.LineNo).ToList();

            decimal expectedDocTotal = lines.Sum(x => x.LineTotal) + invoice.TaxTotal;
            if (expectedDocTotal != invoice.DocTotal)
            {
                throw new InvalidOperationException(
                    "AR invoice totals are inconsistent and cannot be posted."
                );
            }

            PostingRule rule = await postingRuleProvider.GetActiveRuleAsync(
                DocKind.ArInvoice,
                cancellationToken
            );

            ArInvoicePostingContext context = await arInvoicePostingContextBuilder.BuildAsync(
                invoice,
                postingDate,
                period,
                settings,
                rule,
                cancellationToken
            );

            PostingEngineResult postingResult = postingEngine.PostArInvoice(context);

            ValidatePostingResult(
                postingResult,
                PostingSourceTypes.ArInvoice,
                inventoryRowsAllowed: true
            );
            await ValidateAccountsAsync(postingResult.GlEntries, cancellationToken);
            await PersistPostingRowsAsync(postingResult, command.PerformedBy);

            invoice.DocStatus = DocStatus.Posted;
            invoice.PostingDate = postingDate;
            invoice.UpdatedBy = command.PerformedBy;
            invoice.UpdatedAt = DateTimeOffset.UtcNow;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync();

            return new PostResult(
                command.DocKind,
                invoice.Id,
                invoice.DocNo,
                postingDate,
                period.Id,
                postingResult.GlEntries.Count,
                postingResult.InventoryMovements.Count
            );
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackAsync();
            throw new InvalidOperationException(
                "The AR invoice was modified during posting. It may already be posted.",
                ex
            );
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task<PostResult> PostArReceiptAsync(
        PostCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.Force)
        {
            throw new InvalidOperationException(
                "Force posting is not supported for AR receipt posting."
            );
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            ArReceipt receipt =
                await arReceiptRepository.GetTrackedForPostingAsync(
                    command.SourceId,
                    cancellationToken
                ) ?? throw new InvalidOperationException("AR receipt was not found.");

            if (receipt.DocStatus != DocStatus.Draft)
            {
                throw new InvalidOperationException("Only draft AR receipts can be posted.");
            }

            GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                cancellationToken
            );
            DateOnly postingDate = command.PostingDate ?? receipt.ReceiptDate;

            if (
                !ArSettlementCalculator.MatchesCurrency(
                    receipt.CurrencyCode,
                    settings.BaseCurrencyCode
                )
            )
            {
                throw new InvalidOperationException(
                    "AR receipt posting currently supports only receipts in the base currency."
                );
            }

            if (receipt.BankAccount is null)
            {
                throw new InvalidOperationException("AR receipt bank account was not found.");
            }

            if (!receipt.BankAccount.IsActive)
            {
                throw new InvalidOperationException(
                    "AR receipt posting requires an active bank account."
                );
            }

            if (
                !ArSettlementCalculator.MatchesCurrency(
                    receipt.BankAccount.CurrencyCode,
                    receipt.CurrencyCode
                )
            )
            {
                throw new InvalidOperationException(
                    "AR receipt bank account currency must match the receipt currency."
                );
            }

            decimal allocatedAmount = ArSettlementCalculator.GetReceiptAllocatedAmount(receipt);
            if (allocatedAmount > receipt.Amount)
            {
                throw new InvalidOperationException(
                    "AR receipt allocations exceed the receipt amount and cannot be posted."
                );
            }

            FiscalPeriod period =
                await fiscalPeriodRepository.GetOpenForDateAsync(postingDate, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"No open fiscal period exists for posting date {postingDate:yyyy-MM-dd}."
                );

            PostingRule rule = await postingRuleProvider.GetActiveRuleAsync(
                DocKind.ArReceipt,
                cancellationToken
            );

            ArReceiptPostingContext context = await arReceiptPostingContextBuilder.BuildAsync(
                receipt,
                postingDate,
                period,
                settings,
                rule,
                cancellationToken
            );

            PostingEngineResult postingResult = postingEngine.PostArReceipt(context);

            ValidatePostingResult(
                postingResult,
                PostingSourceTypes.ArReceipt,
                inventoryRowsAllowed: false
            );
            await ValidateAccountsAsync(postingResult.GlEntries, cancellationToken);
            await PersistPostingRowsAsync(postingResult, command.PerformedBy);

            receipt.DocStatus = DocStatus.Posted;
            receipt.PostingDate = postingDate;
            receipt.UpdatedBy = command.PerformedBy;
            receipt.UpdatedAt = DateTimeOffset.UtcNow;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync();

            return new PostResult(
                command.DocKind,
                receipt.Id,
                receipt.DocNo,
                postingDate,
                period.Id,
                postingResult.GlEntries.Count,
                postingResult.InventoryMovements.Count
            );
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackAsync();
            throw new InvalidOperationException(
                "The AR receipt was modified during posting. It may already be posted.",
                ex
            );
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task PersistPostingRowsAsync(
        PostingEngineResult postingResult,
        string performedBy
    )
    {
        foreach (GlEntryModel entry in postingResult.GlEntries)
        {
            await glEntryRepository.AddAsync(
                new GlEntry
                {
                    EntryDate = entry.EntryDate,
                    PeriodId = entry.PeriodId,
                    AccountNo = entry.AccountNo,
                    Debit = entry.Debit,
                    Credit = entry.Credit,
                    Description = entry.Description,
                    SourceType = entry.SourceType,
                    SourceId = entry.SourceId,
                    SourceNo = entry.SourceNo,
                    CreatedBy = performedBy,
                }
            );
        }

        foreach (var movement in postingResult.InventoryMovements)
        {
            await inventoryLedgerRepository.AddAsync(
                new Domain.Entities.Inventory.InventoryLedger
                {
                    TrxDate = movement.TrxDate,
                    ItemId = movement.ItemId,
                    LocationId = movement.LocationId,
                    TransactionType = movement.TransactionType,
                    Qty = movement.Qty,
                    UnitCost = movement.UnitCost,
                    ValueChange = movement.ValueChange,
                    SourceType = movement.SourceType,
                    SourceId = movement.SourceId,
                    Note = movement.Note,
                    CreatedBy = performedBy,
                }
            );
        }
    }

    private async Task ValidateAccountsAsync(
        IReadOnlyList<GlEntryModel> entries,
        CancellationToken cancellationToken
    )
    {
        foreach (
            string accountNo in entries.Select(x => x.AccountNo).Distinct(StringComparer.Ordinal)
        )
        {
            var account = await glAccountRepository.FindNoTrackingAsync(
                accountNo,
                cancellationToken
            );
            if (account is null || !account.IsActive)
            {
                throw new InvalidOperationException(
                    $"GL account '{accountNo}' is missing or inactive for posting."
                );
            }
        }
    }

    private static void ValidatePostingResult(
        PostingEngineResult postingResult,
        string expectedSourceType,
        bool inventoryRowsAllowed
    )
    {
        if (postingResult.GlEntries.Count == 0)
        {
            throw new InvalidOperationException("Posting did not produce any GL entries.");
        }

        decimal debit = 0m;
        decimal credit = 0m;

        foreach (GlEntryModel entry in postingResult.GlEntries)
        {
            ValidateTraceability(
                entry.SourceType,
                entry.SourceId,
                entry.SourceNo,
                expectedSourceType
            );

            if (entry.Debit < 0m || entry.Credit < 0m)
            {
                throw new InvalidOperationException("Posting produced negative GL amounts.");
            }

            bool validOneSided =
                (entry.Debit > 0m && entry.Credit == 0m)
                || (entry.Credit > 0m && entry.Debit == 0m);
            if (!validOneSided)
            {
                throw new InvalidOperationException(
                    "Posting produced a GL row that is not one-sided and positive."
                );
            }

            debit += entry.Debit;
            credit += entry.Credit;
        }

        if (debit != credit)
        {
            throw new InvalidOperationException("Posting produced unbalanced GL entries.");
        }

        if (!inventoryRowsAllowed)
        {
            if (postingResult.InventoryMovements.Count > 0)
            {
                throw new InvalidOperationException(
                    "AR receipt posting produced unexpected inventory movements."
                );
            }

            return;
        }

        foreach (var movement in postingResult.InventoryMovements)
        {
            ValidateTraceability(
                movement.SourceType,
                movement.SourceId,
                movement.Note,
                expectedSourceType
            );

            if (movement.TransactionType != InventoryTransactionType.SalesCogs)
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced an inventory movement with an unexpected transaction type."
                );
            }

            if (movement.Qty >= 0m)
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced a non-negative inventory movement."
                );
            }

            if (movement.UnitCost < 0m)
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced a negative inventory unit cost."
                );
            }

            decimal expectedValueChange = Math.Round(
                movement.Qty * movement.UnitCost,
                2,
                MidpointRounding.AwayFromZero
            );
            if (movement.ValueChange != expectedValueChange)
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced an inventory value change that does not match quantity and unit cost."
                );
            }

            if (movement.ValueChange >= 0m)
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced a non-negative inventory value change."
                );
            }
        }
    }

    private static void ValidateTraceability(
        string sourceType,
        Guid sourceId,
        string? sourceText,
        string expectedSourceType
    )
    {
        if (string.IsNullOrWhiteSpace(sourceType))
        {
            throw new InvalidOperationException("Posting produced a row without a source type.");
        }

        if (!string.Equals(sourceType, expectedSourceType, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Posting produced a row with an unexpected source type."
            );
        }

        if (sourceId == Guid.Empty)
        {
            throw new InvalidOperationException("Posting produced a row without a source id.");
        }

        if (string.IsNullOrWhiteSpace(sourceText))
        {
            throw new InvalidOperationException(
                "Posting produced a row without traceability text."
            );
        }
    }
}
