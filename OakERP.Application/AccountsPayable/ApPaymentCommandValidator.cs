using System.Net;
using OakERP.Application.AccountsPayable;

namespace OakERP.Application.AccountsPayable;

public static class ApPaymentCommandValidator
{
    public static ApPaymentCreateValidationResult ValidateCreate(CreateApPaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        string docNo = command.DocNo.Trim();
        string? memo = NormalizeOptional(command.Memo);
        string performedBy = GetPerformedBy(command.PerformedBy);
        IReadOnlyList<ApPaymentAllocationInputDto> allocations = command.Allocations ?? [];

        return new ApPaymentCreateValidationResult(
            ValidateCreateRequest(command, docNo, memo, allocations),
            docNo,
            memo,
            performedBy,
            allocations
        );
    }

    public static ApPaymentAllocateValidationResult ValidateAllocate(
        AllocateApPaymentCommand command
    )
    {
        ArgumentNullException.ThrowIfNull(command);

        IReadOnlyList<ApPaymentAllocationInputDto> allocations = command.Allocations ?? [];

        return new ApPaymentAllocateValidationResult(
            ValidateAllocateRequest(command, allocations),
            GetPerformedBy(command.PerformedBy),
            allocations
        );
    }

    private static ApPaymentCommandResultDto? ValidateCreateRequest(
        CreateApPaymentCommand command,
        string docNo,
        string? memo,
        IReadOnlyList<ApPaymentAllocationInputDto> allocations
    )
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return ApPaymentCommandResultDto.Fail(
                "Document number is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (docNo.Length > 40)
        {
            return ApPaymentCommandResultDto.Fail(
                "Document number may not exceed 40 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.VendorId == Guid.Empty)
        {
            return ApPaymentCommandResultDto.Fail(
                "Vendor id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.BankAccountId == Guid.Empty)
        {
            return ApPaymentCommandResultDto.Fail(
                "Bank account id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.PaymentDate == default)
        {
            return ApPaymentCommandResultDto.Fail(
                "Payment date is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.Amount <= 0m)
        {
            return ApPaymentCommandResultDto.Fail(
                "Payment amount must be greater than zero.",
                HttpStatusCode.BadRequest
            );
        }

        if (memo is not null && memo.Length > 512)
        {
            return ApPaymentCommandResultDto.Fail(
                "Payment memo may not exceed 512 characters.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: true);
    }

    private static ApPaymentCommandResultDto? ValidateAllocateRequest(
        AllocateApPaymentCommand command,
        IReadOnlyList<ApPaymentAllocationInputDto> allocations
    )
    {
        if (command.PaymentId == Guid.Empty)
        {
            return ApPaymentCommandResultDto.Fail(
                "Payment id is required.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: false);
    }

    private static ApPaymentCommandResultDto? ValidateAllocationInputs(
        IReadOnlyList<ApPaymentAllocationInputDto> allocations,
        bool allowEmpty
    )
    {
        if (!allowEmpty && allocations.Count == 0)
        {
            return ApPaymentCommandResultDto.Fail(
                "At least one allocation is required.",
                HttpStatusCode.BadRequest
            );
        }

        HashSet<Guid> invoiceIds = [];

        foreach (ApPaymentAllocationInputDto allocation in allocations)
        {
            if (allocation.ApInvoiceId == Guid.Empty)
            {
                return ApPaymentCommandResultDto.Fail(
                    "Allocation invoice id is required.",
                    HttpStatusCode.BadRequest
                );
            }

            if (!invoiceIds.Add(allocation.ApInvoiceId))
            {
                return ApPaymentCommandResultDto.Fail(
                    "Each invoice may appear only once per allocation request.",
                    HttpStatusCode.BadRequest
                );
            }

            if (allocation.AmountApplied <= 0m)
            {
                return ApPaymentCommandResultDto.Fail(
                    "Allocation amount must be greater than zero.",
                    HttpStatusCode.BadRequest
                );
            }
        }

        return null;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GetPerformedBy(string? performedBy) =>
        string.IsNullOrWhiteSpace(performedBy) ? "system" : performedBy.Trim();
}

public sealed record ApPaymentCreateValidationResult(
    ApPaymentCommandResultDto? Failure,
    string DocNo,
    string? Memo,
    string PerformedBy,
    IReadOnlyList<ApPaymentAllocationInputDto> Allocations
);

public sealed record ApPaymentAllocateValidationResult(
    ApPaymentCommandResultDto? Failure,
    string PerformedBy,
    IReadOnlyList<ApPaymentAllocationInputDto> Allocations
);
