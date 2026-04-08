using OakERP.Common.Dtos.Base;
using OakERP.Common.Errors;

namespace OakERP.Application.AccountsPayable.Invoices.Contracts;

public sealed class ApInvoiceCommandResultDto : BaseResultDto
{
    public ApInvoiceSnapshotDto? Invoice { get; set; }

    public static ApInvoiceCommandResultDto SuccessWith(
        ApInvoiceSnapshotDto invoice,
        string message
    ) =>
        new()
        {
            Success = true,
            Invoice = invoice,
            Message = message,
        };

    public static ApInvoiceCommandResultDto Fail(string code, string message, FailureKind kind) =>
        Fail<ApInvoiceCommandResultDto>(code, message, kind);

    public static ApInvoiceCommandResultDto Fail(ResultError error) =>
        Fail<ApInvoiceCommandResultDto>(error);
}
