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

namespace OakERP.Infrastructure.Posting;

public sealed class PostingService(
    IArInvoiceRepository arInvoiceRepository,
    IFiscalPeriodRepository fiscalPeriodRepository,
    IGlAccountRepository glAccountRepository,
    IGlEntryRepository glEntryRepository,
    IGlSettingsProvider glSettingsProvider,
    IPostingRuleProvider postingRuleProvider,
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
            throw new InvalidOperationException("Force posting is not supported for Slice 1A.");
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            ArInvoice invoice =
                await arInvoiceRepository.GetTrackedForPostingAsync(command.SourceId, cancellationToken)
                ?? throw new InvalidOperationException("AR invoice was not found.");

            if (invoice.DocStatus != DocStatus.Draft)
            {
                throw new InvalidOperationException("Only draft AR invoices can be posted.");
            }

            GlPostingSettings settings = await glSettingsProvider.GetSettingsAsync(cancellationToken);
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
                    "Slice 1A only supports AR invoices in the base currency."
                );
            }

            FiscalPeriod period =
                await fiscalPeriodRepository.GetOpenForDateAsync(postingDate, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"No open fiscal period exists for posting date {postingDate:yyyy-MM-dd}."
                );

            IReadOnlyList<ArInvoiceLine> lines = invoice.Lines.OrderBy(x => x.LineNo).ToList();

            if (lines.Any(x => x.Item?.Type == ItemType.Stock))
            {
                throw new InvalidOperationException(
                    "Slice 1A does not support AR invoice posting for stock lines."
                );
            }

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

            PostingEngineResult postingResult = postingEngine.PostArInvoice(
                new ArInvoicePostingContext(
                    invoice,
                    lines,
                    postingDate,
                    period,
                    settings.BaseCurrencyCode,
                    1m,
                    settings,
                    rule
                )
            );

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
                0
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
    ) => throw new NotSupportedException("Unposting is not supported for Slice 1A.");

    private async Task ValidateAccountsAsync(
        IReadOnlyList<GlEntryModel> entries,
        CancellationToken cancellationToken
    )
    {
        foreach (string accountNo in entries.Select(x => x.AccountNo).Distinct(StringComparer.Ordinal))
        {
            var account = await glAccountRepository.FindNoTrackingAsync(accountNo, cancellationToken);
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
        if (postingResult.InventoryMovements.Count != 0)
        {
            throw new InvalidOperationException("Slice 1A must not produce inventory movements.");
        }

        if (postingResult.GlEntries.Count == 0)
        {
            throw new InvalidOperationException("Posting did not produce any GL entries.");
        }

        decimal debit = 0m;
        decimal credit = 0m;

        foreach (GlEntryModel entry in postingResult.GlEntries)
        {
            if (entry.Debit < 0m || entry.Credit < 0m)
            {
                throw new InvalidOperationException("Posting produced negative GL amounts.");
            }

            bool validOneSided =
                (entry.Debit > 0m && entry.Credit == 0m) || (entry.Credit > 0m && entry.Debit == 0m);
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
    }
}
