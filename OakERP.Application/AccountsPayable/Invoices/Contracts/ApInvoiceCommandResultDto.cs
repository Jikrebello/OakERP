using System.Net;
using OakERP.Common.Dtos.Base;

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

    public static ApInvoiceCommandResultDto Fail(string message, HttpStatusCode statusCode) =>
        Fail<ApInvoiceCommandResultDto>(message, statusCode);
}
