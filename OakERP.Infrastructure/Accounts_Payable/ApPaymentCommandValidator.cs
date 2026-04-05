using System.Net;
using OakERP.Application.AccountsPayable;

namespace OakERP.Infrastructure.Accounts_Payable;

public sealed class ApPaymentCommandValidator
{
    public ApPaymentCreateValidationResult ValidateCreate(CreateApPaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        string docNo = command.DocNo.Trim();
        string? memo = NormalizeOptional(command.Memo);
        string performedBy = GetPerformedBy(command.PerformedBy);
        IReadOnlyList<ApPaymentAllocationInputDTO> allocations = command.Allocations ?? [];

        return new ApPaymentCreateValidationResult(
            ValidateCreateRequest(command, docNo, memo, allocations),
            docNo,
            memo,
            performedBy,
            allocations
        );
    }

    public ApPaymentAllocateValidationResult ValidateAllocate(AllocateApPaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        IReadOnlyList<ApPaymentAllocationInputDTO> allocations = command.Allocations ?? [];

        return new ApPaymentAllocateValidationResult(
            ValidateAllocateRequest(command, allocations),
            GetPerformedBy(command.PerformedBy),
            allocations
        );
    }

    private static ApPaymentCommandResultDTO? ValidateCreateRequest(
        CreateApPaymentCommand command,
        string docNo,
        string? memo,
        IReadOnlyList<ApPaymentAllocationInputDTO> allocations
    )
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return ApPaymentCommandResultDTO.Fail(
                "Document number is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (docNo.Length > 40)
        {
            return ApPaymentCommandResultDTO.Fail(
                "Document number may not exceed 40 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.VendorId == Guid.Empty)
        {
            return ApPaymentCommandResultDTO.Fail(
                "Vendor id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.BankAccountId == Guid.Empty)
        {
            return ApPaymentCommandResultDTO.Fail(
                "Bank account id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.PaymentDate == default)
        {
            return ApPaymentCommandResultDTO.Fail(
                "Payment date is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.Amount <= 0m)
        {
            return ApPaymentCommandResultDTO.Fail(
                "Payment amount must be greater than zero.",
                HttpStatusCode.BadRequest
            );
        }

        if (memo is not null && memo.Length > 512)
        {
            return ApPaymentCommandResultDTO.Fail(
                "Payment memo may not exceed 512 characters.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: true);
    }

    private static ApPaymentCommandResultDTO? ValidateAllocateRequest(
        AllocateApPaymentCommand command,
        IReadOnlyList<ApPaymentAllocationInputDTO> allocations
    )
    {
        if (command.PaymentId == Guid.Empty)
        {
            return ApPaymentCommandResultDTO.Fail(
                "Payment id is required.",
                HttpStatusCode.BadRequest
            );
        }

        return ValidateAllocationInputs(allocations, allowEmpty: false);
    }

    private static ApPaymentCommandResultDTO? ValidateAllocationInputs(
        IReadOnlyList<ApPaymentAllocationInputDTO> allocations,
        bool allowEmpty
    )
    {
        if (!allowEmpty && allocations.Count == 0)
        {
            return ApPaymentCommandResultDTO.Fail(
                "At least one allocation is required.",
                HttpStatusCode.BadRequest
            );
        }

        HashSet<Guid> invoiceIds = [];

        foreach (ApPaymentAllocationInputDTO allocation in allocations)
        {
            if (allocation.ApInvoiceId == Guid.Empty)
            {
                return ApPaymentCommandResultDTO.Fail(
                    "Allocation invoice id is required.",
                    HttpStatusCode.BadRequest
                );
            }

            if (!invoiceIds.Add(allocation.ApInvoiceId))
            {
                return ApPaymentCommandResultDTO.Fail(
                    "Each invoice may appear only once per allocation request.",
                    HttpStatusCode.BadRequest
                );
            }

            if (allocation.AmountApplied <= 0m)
            {
                return ApPaymentCommandResultDTO.Fail(
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
    ApPaymentCommandResultDTO? Failure,
    string DocNo,
    string? Memo,
    string PerformedBy,
    IReadOnlyList<ApPaymentAllocationInputDTO> Allocations
);

public sealed record ApPaymentAllocateValidationResult(
    ApPaymentCommandResultDTO? Failure,
    string PerformedBy,
    IReadOnlyList<ApPaymentAllocationInputDTO> Allocations
);
