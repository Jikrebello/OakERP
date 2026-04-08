using Microsoft.EntityFrameworkCore;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
using OakERP.Domain.Accounts_Payable;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Entities.Accounts_Payable;
using OakERP.Domain.Entities.Accounts_Receivable;
using OakERP.Domain.Entities.General_Ledger;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.Accounts_Payable;
using OakERP.Domain.Posting.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Domain.Posting.Inventory;
using OakERP.Domain.Repository_Interfaces.Accounts_Payable;
using OakERP.Domain.Repository_Interfaces.Accounts_Receivable;
using OakERP.Domain.Repository_Interfaces.General_Ledger;
using OakERP.Domain.Repository_Interfaces.Inventory;

namespace OakERP.Infrastructure.Posting;

public sealed class PostingService(
    PostingSourceRepositories sourceRepositories,
    PostingPersistenceDependencies persistenceDependencies,
    PostingRuntimeDependencies runtimeDependencies,
    PostingContextBuilders contextBuilders
) : IPostingService
{
    private IApPaymentRepository apPaymentRepository => sourceRepositories.ApPaymentRepository;
    private IApInvoiceRepository apInvoiceRepository => sourceRepositories.ApInvoiceRepository;
    private IArInvoiceRepository arInvoiceRepository => sourceRepositories.ArInvoiceRepository;
    private IArReceiptRepository arReceiptRepository => sourceRepositories.ArReceiptRepository;
    private IFiscalPeriodRepository fiscalPeriodRepository =>
        persistenceDependencies.FiscalPeriodRepository;
    private IGlAccountRepository glAccountRepository => persistenceDependencies.GlAccountRepository;
    private IGlEntryRepository glEntryRepository => persistenceDependencies.GlEntryRepository;
    private IInventoryLedgerRepository inventoryLedgerRepository =>
        persistenceDependencies.InventoryLedgerRepository;
    private IUnitOfWork unitOfWork => persistenceDependencies.UnitOfWork;
    private IGlSettingsProvider glSettingsProvider => runtimeDependencies.GlSettingsProvider;
    private IPostingRuleProvider postingRuleProvider => runtimeDependencies.PostingRuleProvider;
    private IPostingEngine postingEngine => runtimeDependencies.PostingEngine;
    private IApPaymentPostingContextBuilder apPaymentPostingContextBuilder =>
        contextBuilders.ApPaymentPostingContextBuilder;
    private IApInvoicePostingContextBuilder apInvoicePostingContextBuilder =>
        contextBuilders.ApInvoicePostingContextBuilder;
    private IArInvoicePostingContextBuilder arInvoicePostingContextBuilder =>
        contextBuilders.ArInvoicePostingContextBuilder;
    private IArReceiptPostingContextBuilder arReceiptPostingContextBuilder =>
        contextBuilders.ArReceiptPostingContextBuilder;

    public Task<PostResult> PostAsync(
        PostCommand command,
        CancellationToken cancellationToken = default
    ) =>
        command.DocKind switch
        {
            DocKind.ApPayment => PostApPaymentAsync(command, cancellationToken),
            DocKind.ApInvoice => PostApInvoiceAsync(command, cancellationToken),
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

    private async Task<PostResult> PostApPaymentAsync(
        PostCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.Force)
        {
            throw new InvalidOperationException(
                "Force posting is not supported for AP payment posting."
            );
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            ApPayment payment =
                await apPaymentRepository.GetTrackedForPostingAsync(
                    command.SourceId,
                    cancellationToken
                ) ?? throw new InvalidOperationException("AP payment was not found.");

            if (payment.DocStatus != DocStatus.Draft)
            {
                throw new InvalidOperationException("Only draft AP payments can be posted.");
            }

            GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                cancellationToken
            );
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

            if (
                !ApSettlementCalculator.MatchesCurrency(
                    payment.BankAccount.CurrencyCode,
                    settings.BaseCurrencyCode
                )
            )
            {
                throw new InvalidOperationException(
                    "AP payment posting currently supports only payments in the base currency."
                );
            }

            decimal allocatedAmount = ApSettlementCalculator.GetPaymentAllocatedAmount(payment);
            if (allocatedAmount > payment.Amount)
            {
                throw new InvalidOperationException(
                    "AP payment allocations exceed the payment amount and cannot be posted."
                );
            }

            FiscalPeriod period =
                await fiscalPeriodRepository.GetOpenForDateAsync(postingDate, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"No open fiscal period exists for posting date {postingDate:yyyy-MM-dd}."
                );

            PostingRule rule = await postingRuleProvider.GetActiveRuleAsync(
                DocKind.ApPayment,
                cancellationToken
            );

            ApPaymentPostingContext context = await apPaymentPostingContextBuilder.BuildAsync(
                payment,
                postingDate,
                period,
                settings,
                rule,
                cancellationToken
            );

            PostingEngineResult postingResult = postingEngine.PostApPayment(context);

            ValidatePostingResult(
                postingResult,
                PostingSourceTypes.ApPayment,
                inventoryRowsAllowed: false
            );
            await ValidateAccountsAsync(postingResult.GlEntries, cancellationToken);
            await PersistPostingRowsAsync(postingResult, command.PerformedBy);

            payment.DocStatus = DocStatus.Posted;
            payment.PostingDate = postingDate;
            payment.UpdatedBy = command.PerformedBy;
            payment.UpdatedAt = DateTimeOffset.UtcNow;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync();

            return new PostResult(
                command.DocKind,
                payment.Id,
                payment.DocNo,
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
                "The AP payment was modified during posting. It may already be posted.",
                ex
            );
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task<PostResult> PostApInvoiceAsync(
        PostCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.Force)
        {
            throw new InvalidOperationException(
                "Force posting is not supported for AP invoice posting."
            );
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            ApInvoice invoice =
                await apInvoiceRepository.GetTrackedForPostingAsync(
                    command.SourceId,
                    cancellationToken
                ) ?? throw new InvalidOperationException("AP invoice was not found.");

            if (invoice.DocStatus != DocStatus.Draft)
            {
                throw new InvalidOperationException("Only draft AP invoices can be posted.");
            }

            GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(
                cancellationToken
            );
            DateOnly postingDate = command.PostingDate ?? invoice.InvoiceDate;

            if (
                !string.Equals(
                    invoice.CurrencyCode,
                    settings.BaseCurrencyCode,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                throw new InvalidOperationException(
                    "AP invoice posting currently supports only invoices in the base currency."
                );
            }

            FiscalPeriod period =
                await fiscalPeriodRepository.GetOpenForDateAsync(postingDate, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"No open fiscal period exists for posting date {postingDate:yyyy-MM-dd}."
                );

            IReadOnlyList<ApInvoiceLine> lines = [.. invoice.Lines.OrderBy(x => x.LineNo)];
            if (lines.Count == 0)
            {
                throw new InvalidOperationException(
                    "AP invoice requires at least one line to be posted."
                );
            }

            decimal expectedDocTotal = lines.Sum(x => x.LineTotal) + invoice.TaxTotal;
            if (expectedDocTotal != invoice.DocTotal)
            {
                throw new InvalidOperationException(
                    "AP invoice totals are inconsistent and cannot be posted."
                );
            }

            PostingRule rule = await postingRuleProvider.GetActiveRuleAsync(
                DocKind.ApInvoice,
                cancellationToken
            );

            ApInvoicePostingContext context = await apInvoicePostingContextBuilder.BuildAsync(
                invoice,
                postingDate,
                period,
                settings,
                rule,
                cancellationToken
            );

            PostingEngineResult postingResult = postingEngine.PostApInvoice(context);

            ValidatePostingResult(
                postingResult,
                PostingSourceTypes.ApInvoice,
                inventoryRowsAllowed: false
            );
            await ValidateAccountsAsync(postingResult.GlEntries, cancellationToken);
            await PersistPostingRowsAsync(postingResult, command.PerformedBy);

            invoice.DocStatus = DocStatus.Posted;
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
                "The AP invoice was modified during posting. It may already be posted.",
                ex
            );
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

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

            IReadOnlyList<ArInvoiceLine> lines = [.. invoice.Lines.OrderBy(x => x.LineNo)];

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
        ValidateGlEntries(postingResult.GlEntries, expectedSourceType);

        if (!inventoryRowsAllowed)
        {
            EnsureNoUnexpectedInventoryMovements(postingResult.InventoryMovements);
            return;
        }

        ValidateInventoryMovements(postingResult.InventoryMovements, expectedSourceType);
    }

    private static void ValidateGlEntries(
        IReadOnlyCollection<GlEntryModel> glEntries,
        string expectedSourceType
    )
    {
        if (glEntries.Count == 0)
        {
            throw new InvalidOperationException("Posting did not produce any GL entries.");
        }

        decimal debit = 0m;
        decimal credit = 0m;

        foreach (GlEntryModel entry in glEntries)
        {
            ValidateTraceability(
                entry.SourceType,
                entry.SourceId,
                entry.SourceNo,
                expectedSourceType
            );
            ValidateGlEntryAmounts(entry);

            debit += entry.Debit;
            credit += entry.Credit;
        }

        if (debit != credit)
        {
            throw new InvalidOperationException("Posting produced unbalanced GL entries.");
        }
    }

    private static void ValidateGlEntryAmounts(GlEntryModel entry)
    {
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
    }

    private static void EnsureNoUnexpectedInventoryMovements(
        IReadOnlyCollection<InventoryMovementModel> inventoryMovements
    )
    {
        if (inventoryMovements.Count > 0)
        {
            throw new InvalidOperationException(
                "Posting produced unexpected inventory movements."
            );
        }
    }

    private static void ValidateInventoryMovements(
        IReadOnlyCollection<InventoryMovementModel> inventoryMovements,
        string expectedSourceType
    )
    {
        foreach (InventoryMovementModel movement in inventoryMovements)
        {
            ValidateTraceability(
                movement.SourceType,
                movement.SourceId,
                movement.Note,
                expectedSourceType
            );
            ValidateInventoryMovement(movement);
        }
    }

    private static void ValidateInventoryMovement(InventoryMovementModel movement)
    {
        if (movement.TransactionType != InventoryTransactionType.SalesCogs)
        {
            throw new InvalidOperationException(
                "Posting produced an inventory movement with an unexpected transaction type."
            );
        }

        if (movement.Qty >= 0m)
        {
            throw new InvalidOperationException(
                "Posting produced a non-negative inventory movement."
            );
        }

        if (movement.UnitCost < 0m)
        {
            throw new InvalidOperationException(
                "Posting produced a negative inventory unit cost."
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
                "Posting produced an inventory value change that does not match quantity and unit cost."
            );
        }

        if (movement.ValueChange >= 0m)
        {
            throw new InvalidOperationException(
                "Posting produced a non-negative inventory value change."
            );
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
