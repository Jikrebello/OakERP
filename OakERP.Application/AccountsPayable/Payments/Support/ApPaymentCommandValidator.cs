namespace OakERP.Application.AccountsPayable.Payments.Support;

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
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.DocumentNumberRequired);
        }

        if (docNo.Length > 40)
        {
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.DocumentNumberTooLong);
        }

        if (command.VendorId == Guid.Empty)
        {
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.VendorIdRequired);
        }

        if (command.BankAccountId == Guid.Empty)
        {
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.BankAccountIdRequired);
        }

        if (command.PaymentDate == default)
        {
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.PaymentDateRequired);
        }

        if (command.Amount <= 0m)
        {
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.PaymentAmountInvalid);
        }

        if (memo is not null && memo.Length > 512)
        {
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.MemoTooLong);
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
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.PaymentIdRequired);
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
            return ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationRequired);
        }

        HashSet<Guid> invoiceIds = [];

        foreach (ApPaymentAllocationInputDto allocation in allocations)
        {
            if (allocation.ApInvoiceId == Guid.Empty)
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationInvoiceIdRequired);
            }

            if (!invoiceIds.Add(allocation.ApInvoiceId))
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationDuplicateInvoice);
            }

            if (allocation.AmountApplied <= 0m)
            {
                return ApPaymentCommandResultDto.Fail(ApPaymentErrors.AllocationAmountInvalid);
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
