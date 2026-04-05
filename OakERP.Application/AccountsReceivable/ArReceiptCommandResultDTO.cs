using System.Net;
using OakERP.Common.DTOs.Base;

namespace OakERP.Application.AccountsReceivable;

public sealed class ArReceiptCommandResultDTO : BaseResultDTO
{
    public ArReceiptSnapshotDTO? Receipt { get; set; }
    public IReadOnlyList<ArInvoiceSettlementSnapshotDTO> Invoices { get; set; } = [];

    public static ArReceiptCommandResultDTO SuccessWith(
        ArReceiptSnapshotDTO receipt,
        IReadOnlyList<ArInvoiceSettlementSnapshotDTO> invoices,
        string message
    ) =>
        new()
        {
            Success = true,
            Receipt = receipt,
            Invoices = invoices,
            Message = message,
        };

    public static ArReceiptCommandResultDTO Fail(string message, HttpStatusCode statusCode) =>
        Fail<ArReceiptCommandResultDTO>(message, statusCode);
}
