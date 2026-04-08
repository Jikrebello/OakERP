using OakERP.Common.Dtos.Base;
using OakERP.Common.Errors;

namespace OakERP.Application.AccountsReceivable.Invoices.Contracts;

public sealed class ArInvoiceCommandResultDto : BaseResultDto
{
    public ArInvoiceSnapshotDto? Invoice { get; set; }

    public static ArInvoiceCommandResultDto SuccessWith(
        ArInvoiceSnapshotDto invoice,
        string message
    ) =>
        new()
        {
            Success = true,
            Invoice = invoice,
            Message = message,
        };

    public static ArInvoiceCommandResultDto Fail(string code, string message, FailureKind kind) =>
        Fail<ArInvoiceCommandResultDto>(code, message, kind);

    public static ArInvoiceCommandResultDto Fail(ResultError error) =>
        Fail<ArInvoiceCommandResultDto>(error);
}
