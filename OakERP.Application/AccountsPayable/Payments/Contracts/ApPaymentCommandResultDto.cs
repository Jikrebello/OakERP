using OakERP.Common.Dtos.Base;
using OakERP.Common.Errors;

namespace OakERP.Application.AccountsPayable.Payments.Contracts;

public sealed class ApPaymentCommandResultDto : BaseResultDto
{
    public ApPaymentSnapshotDto? Payment { get; set; }
    public IReadOnlyList<ApInvoiceSettlementSnapshotDto>? Invoices { get; set; }

    public static ApPaymentCommandResultDto SuccessWith(
        ApPaymentSnapshotDto payment,
        IReadOnlyList<ApInvoiceSettlementSnapshotDto>? invoices,
        string message
    ) =>
        new()
        {
            Success = true,
            Payment = payment,
            Invoices = invoices,
            Message = message,
        };

    public static ApPaymentCommandResultDto Fail(string code, string message, FailureKind kind) =>
        Fail<ApPaymentCommandResultDto>(code, message, kind);

    public static ApPaymentCommandResultDto Fail(ResultError error) =>
        Fail<ApPaymentCommandResultDto>(error);
}
