using System.Net;
using OakERP.Application.AccountsReceivable;
using OakERP.Domain.Accounts_Receivable;
using OakERP.Domain.Posting.General_Ledger;

namespace OakERP.Infrastructure.Accounts_Receivable;

public static class ArReceiptCommandValidator
{
    public static ArReceiptCreateValidationResult ValidateCreate(
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
        IReadOnlyList<ArReceiptAllocationInputDto> allocations = command.Allocations ?? [];

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

    public static ArReceiptAllocateValidationResult ValidateAllocate(
        AllocateArReceiptCommand command
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        IReadOnlyList<ArReceiptAllocationInputDto> allocations = command.Allocations ?? [];

        return new ArReceiptAllocateValidationResult(
            ValidateAllocateRequest(command, allocations),
            GetPerformedBy(command.PerformedBy),
            allocations
        );
    }

    private static ArReceiptCommandResultDto? ValidateCreateRequest(
        CreateArReceiptCommand command,
        string docNo,
        string? memo,
        string currencyCode,
        string baseCurrencyCode,
        IReadOnlyList<ArReceiptAllocationInputDto> allocations
    )
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return ArReceiptCommandResultDto.Fail(
                "Document number is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (docNo.Length > 40)
        {
            return ArReceiptCommandResultDto.Fail(
                "Document number may not exceed 40 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.CustomerId == Guid.Empty)
        {
            return ArReceiptCommandResultDto.Fail(
                "Customer id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.BankAccountId == Guid.Empty)
        {
            return ArReceiptCommandResultDto.Fail(
                "Bank account id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.Amount <= 0m)
        {
            return ArReceiptCommandResultDto.Fail(
                "Receipt amount must be greater than zero.",
                HttpStatusCode.BadRequest
            );
        }

        if (memo is not null && memo.Length > 512)
        {
            return ArReceiptCommandResultDto.Fail(
                "Receipt memo may not exceed 512 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (!ArSettlementCalculator.MatchesCurrency(currencyCode, baseCurrencyCode))
        {
            return ArReceiptCommandResultDto.Fail(
                "AR receipt capture currently supports only the base currency.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: true);
    }

    private static ArReceiptCommandResultDto? ValidateAllocateRequest(
        AllocateArReceiptCommand command,
        IReadOnlyList<ArReceiptAllocationInputDto> allocations
    )
    {
        if (command.ReceiptId == Guid.Empty)
        {
            return ArReceiptCommandResultDto.Fail(
                "Receipt id is required.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: false);
    }

    private static ArReceiptCommandResultDto? ValidateAllocationInputs(
        IReadOnlyList<ArReceiptAllocationInputDto> allocations,
        bool allowEmpty
    )
    {
        if (!allowEmpty && allocations.Count == 0)
        {
            return ArReceiptCommandResultDto.Fail(
                "At least one allocation is required.",
                HttpStatusCode.BadRequest
            );
        }

        HashSet<Guid> invoiceIds = [];

        foreach (ArReceiptAllocationInputDto allocation in allocations)
        {
            if (allocation.ArInvoiceId == Guid.Empty)
            {
                return ArReceiptCommandResultDto.Fail(
                    "Allocation invoice id is required.",
                    HttpStatusCode.BadRequest
                );
            }

            if (!invoiceIds.Add(allocation.ArInvoiceId))
            {
                return ArReceiptCommandResultDto.Fail(
                    "Each invoice may appear only once per allocation request.",
                    HttpStatusCode.BadRequest
                );
            }

            if (allocation.AmountApplied <= 0m)
            {
                return ArReceiptCommandResultDto.Fail(
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
    ArReceiptCommandResultDto? Failure,
    string DocNo,
    string? Memo,
    string CurrencyCode,
    string PerformedBy,
    IReadOnlyList<ArReceiptAllocationInputDto> Allocations
);

public sealed record ArReceiptAllocateValidationResult(
    ArReceiptCommandResultDto? Failure,
    string PerformedBy,
    IReadOnlyList<ArReceiptAllocationInputDto> Allocations
);
