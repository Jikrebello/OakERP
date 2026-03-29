using Microsoft.EntityFrameworkCore;
using OakERP.Application.Interfaces.Persistence;
using OakERP.Application.Posting;
using OakERP.Common.Enums;
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
    IFiscalPeriodRepository fiscalPeriodRepository,
    IGlAccountRepository glAccountRepository,
    IGlEntryRepository glEntryRepository,
    IInventoryLedgerRepository inventoryLedgerRepository,
    IGlSettingsProvider glSettingsProvider,
    IPostingRuleProvider postingRuleProvider,
    IArInvoicePostingContextBuilder postingContextBuilder,
    IPostingEngine postingEngine,
    IUnitOfWork unitOfWork
) : IPostingService
{
    public async Task<PostResult> PostAsync(
        PostCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (command.DocKind != DocKind.ArInvoice)
        {
            throw new NotSupportedException(
                $"Posting for document kind '{command.DocKind}' is not supported."
            );
        }

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
                !string.Equals(
                    invoice.CurrencyCode,
                    settings.BaseCurrencyCode,
                    StringComparison.OrdinalIgnoreCase
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

            ArInvoicePostingContext context = await postingContextBuilder.BuildAsync(
                invoice,
                postingDate,
                period,
                settings,
                rule,
                cancellationToken
            );

            PostingEngineResult postingResult = postingEngine.PostArInvoice(context);

            ValidatePostingResult(postingResult);
            await ValidateAccountsAsync(postingResult.GlEntries, cancellationToken);

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
                        CreatedBy = command.PerformedBy,
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
                        CreatedBy = command.PerformedBy,
                    }
                );
            }

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

    public Task<UnpostResult> UnpostAsync(
        UnpostCommand command,
        CancellationToken cancellationToken = default
    ) => throw new NotSupportedException("Unposting is not supported for AR invoice posting.");

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

    private static void ValidatePostingResult(PostingEngineResult postingResult)
    {
        if (postingResult.GlEntries.Count == 0)
        {
            throw new InvalidOperationException("Posting did not produce any GL entries.");
        }

        decimal debit = 0m;
        decimal credit = 0m;

        foreach (GlEntryModel entry in postingResult.GlEntries)
        {
            if (string.IsNullOrWhiteSpace(entry.SourceType))
            {
                throw new InvalidOperationException(
                    "Posting produced a GL row without a source type."
                );
            }

            if (
                !string.Equals(
                    entry.SourceType,
                    PostingSourceTypes.ArInvoice,
                    StringComparison.Ordinal
                )
            )
            {
                throw new InvalidOperationException(
                    "Posting produced a GL row with an unexpected source type."
                );
            }

            if (entry.SourceId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Posting produced a GL row without a source id."
                );
            }

            if (string.IsNullOrWhiteSpace(entry.SourceNo))
            {
                throw new InvalidOperationException(
                    "Posting produced a GL row without a source number."
                );
            }

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

        foreach (var movement in postingResult.InventoryMovements)
        {
            if (string.IsNullOrWhiteSpace(movement.SourceType))
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced an inventory movement without a source type."
                );
            }

            if (
                !string.Equals(
                    movement.SourceType,
                    PostingSourceTypes.ArInvoice,
                    StringComparison.Ordinal
                )
            )
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced an inventory movement with an unexpected source type."
                );
            }

            if (movement.SourceId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced an inventory movement without a source id."
                );
            }

            if (string.IsNullOrWhiteSpace(movement.Note))
            {
                throw new InvalidOperationException(
                    "AR invoice posting produced an inventory movement without a trace note."
                );
            }

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
}
