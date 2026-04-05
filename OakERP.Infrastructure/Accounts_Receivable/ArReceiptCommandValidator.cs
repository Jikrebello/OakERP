using System.Net;
using OakERP.Application.AccountsReceivable;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Accounts_Receivable;

public sealed class ArReceiptCommandValidator
{
    public ArReceiptCreateValidationResult ValidateCreate(
        CreateArReceiptCommand command,
        GlPostingSettings settings
    )
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(settings);

        string docNo = command.DocNo.Trim();
        string? memo = NormalizeOptional(command.Memo);
        string currencyCode = NormalizeCurrencyCode(
            command.CurrencyCode,
            settings.BaseCurrencyCode
        );
        string performedBy = GetPerformedBy(command.PerformedBy);
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations = command.Allocations ?? [];

        return new ArReceiptCreateValidationResult(
            ValidateCreateRequest(
                command,
                docNo,
                memo,
                currencyCode,
                settings.BaseCurrencyCode,
                allocations
            ),
            docNo,
            memo,
            currencyCode,
            performedBy,
            allocations
        );
    }

    public ArReceiptAllocateValidationResult ValidateAllocate(AllocateArReceiptCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        IReadOnlyList<ArReceiptAllocationInputDTO> allocations = command.Allocations ?? [];

        return new ArReceiptAllocateValidationResult(
            ValidateAllocateRequest(command, allocations),
            GetPerformedBy(command.PerformedBy),
            allocations
        );
    }

    private static ArReceiptCommandResultDTO? ValidateCreateRequest(
        CreateArReceiptCommand command,
        string docNo,
        string? memo,
        string currencyCode,
        string baseCurrencyCode,
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations
    )
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return ArReceiptCommandResultDTO.Fail(
                "Document number is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (docNo.Length > 40)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Document number may not exceed 40 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.CustomerId == Guid.Empty)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Customer id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.BankAccountId == Guid.Empty)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Bank account id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.Amount <= 0m)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Receipt amount must be greater than zero.",
                HttpStatusCode.BadRequest
            );
        }

        if (memo is not null && memo.Length > 512)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Receipt memo may not exceed 512 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (!ArSettlementCalculator.MatchesCurrency(currencyCode, baseCurrencyCode))
        {
            return ArReceiptCommandResultDTO.Fail(
                "AR receipt capture currently supports only the base currency.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: true);
    }

    private static ArReceiptCommandResultDTO? ValidateAllocateRequest(
        AllocateArReceiptCommand command,
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations
    )
    {
        if (command.ReceiptId == Guid.Empty)
        {
            return ArReceiptCommandResultDTO.Fail(
                "Receipt id is required.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: false);
    }

    private static ArReceiptCommandResultDTO? ValidateAllocationInputs(
        IReadOnlyList<ArReceiptAllocationInputDTO> allocations,
        bool allowEmpty
    )
    {
        if (!allowEmpty && allocations.Count == 0)
        {
            return ArReceiptCommandResultDTO.Fail(
                "At least one allocation is required.",
                HttpStatusCode.BadRequest
            );
        }

        HashSet<Guid> invoiceIds = [];

        foreach (ArReceiptAllocationInputDTO allocation in allocations)
        {
            if (allocation.ArInvoiceId == Guid.Empty)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Allocation invoice id is required.",
                    HttpStatusCode.BadRequest
                );
            }

            if (!invoiceIds.Add(allocation.ArInvoiceId))
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Each invoice may appear only once per allocation request.",
                    HttpStatusCode.BadRequest
                );
            }

            if (allocation.AmountApplied <= 0m)
            {
                return ArReceiptCommandResultDTO.Fail(
                    "Allocation amount must be greater than zero.",
                    HttpStatusCode.BadRequest
                );
            }
        }

        return null;
    }

    private static string NormalizeCurrencyCode(string? currencyCode, string baseCurrencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode)
            ? baseCurrencyCode.ToUpperInvariant()
            : currencyCode.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GetPerformedBy(string? performedBy) =>
        string.IsNullOrWhiteSpace(performedBy) ? "system" : performedBy.Trim();
}

public sealed record ArReceiptCreateValidationResult(
    ArReceiptCommandResultDTO? Failure,
    string DocNo,
    string? Memo,
    string CurrencyCode,
    string PerformedBy,
    IReadOnlyList<ArReceiptAllocationInputDTO> Allocations
);

public sealed record ArReceiptAllocateValidationResult(
    ArReceiptCommandResultDTO? Failure,
    string PerformedBy,
    IReadOnlyList<ArReceiptAllocationInputDTO> Allocations
);
