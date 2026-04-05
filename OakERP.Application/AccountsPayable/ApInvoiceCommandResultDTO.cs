using System.Net;
using OakERP.Common.DTOs.Base;

namespace OakERP.Application.AccountsPayable;

public sealed class ApInvoiceCommandResultDTO : BaseResultDTO
{
    public ApInvoiceSnapshotDTO? Invoice { get; set; }

    public static ApInvoiceCommandResultDTO SuccessWith(
        ApInvoiceSnapshotDTO invoice,
        string message
    ) =>
        new()
        {
            Success = true,
            Invoice = invoice,
            Message = message,
        };

    public static ApInvoiceCommandResultDTO Fail(string message, HttpStatusCode statusCode) =>
        Fail<ApInvoiceCommandResultDTO>(message, statusCode);
}
