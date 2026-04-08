using OakERP.Common.Enums;
using OakERP.Domain.Entities.GeneralLedger;
using OakERP.Domain.Entities.Inventory;
using OakERP.Domain.Posting;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Domain.Posting.Inventory;
using OakERP.Domain.RepositoryInterfaces.GeneralLedger;
using OakERP.Domain.RepositoryInterfaces.Inventory;

namespace OakERP.Application.Posting.Support;

internal sealed class PostingResultProcessor(PostingPersistenceDependencies persistenceDependencies)
{
    private IGlAccountRepository GlAccountRepository => persistenceDependencies.GlAccountRepository;
    private IGlEntryRepository GlEntryRepository => persistenceDependencies.GlEntryRepository;
    private IInventoryLedgerRepository InventoryLedgerRepository =>
        persistenceDependencies.InventoryLedgerRepository;

    public async Task ProcessAsync(
        PostingEngineResult postingResult,
        string expectedSourceType,
        bool inventoryRowsAllowed,
        string performedBy,
        CancellationToken cancellationToken
    )
    {
        ValidatePostingResult(postingResult, expectedSourceType, inventoryRowsAllowed);
        await ValidateAccountsAsync(postingResult.GlEntries, cancellationToken);
        await PersistPostingRowsAsync(postingResult, performedBy);
    }

    private async Task PersistPostingRowsAsync(
        PostingEngineResult postingResult,
        string performedBy
    )
    {
        foreach (GlEntryModel entry in postingResult.GlEntries)
        {
            await GlEntryRepository.AddAsync(
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

        foreach (InventoryMovementModel movement in postingResult.InventoryMovements)
        {
            await InventoryLedgerRepository.AddAsync(
                new InventoryLedger
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
            var account = await GlAccountRepository.FindNoTrackingAsync(accountNo, cancellationToken);
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
