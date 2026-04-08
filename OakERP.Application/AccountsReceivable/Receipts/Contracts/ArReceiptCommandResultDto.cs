using System.Net;
using OakERP.Common.Dtos.Base;
using OakERP.Common.Errors;

namespace OakERP.Application.AccountsReceivable.Receipts.Contracts;

public sealed class ArReceiptCommandResultDto : BaseResultDto
{
    public ArReceiptSnapshotDto? Receipt { get; set; }
    public IReadOnlyList<ArInvoiceSettlementSnapshotDto>? Invoices { get; set; }

    public static ArReceiptCommandResultDto SuccessWith(
        ArReceiptSnapshotDto receipt,
        IReadOnlyList<ArInvoiceSettlementSnapshotDto>? invoices,
        string message
    ) =>
        new()
        {
            Success = true,
            Receipt = receipt,
            Invoices = invoices,
            Message = message,
        };

    public static ArReceiptCommandResultDto Fail(string message, HttpStatusCode statusCode) =>
        Fail<ArReceiptCommandResultDto>(message, statusCode);

    public static ArReceiptCommandResultDto Fail(ResultError error) =>
        Fail<ArReceiptCommandResultDto>(error);
}
