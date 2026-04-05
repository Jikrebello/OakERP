using System.Net;
using OakERP.Common.DTOs.Base;

namespace OakERP.Application.AccountsPayable;

public sealed class ApPaymentCommandResultDTO : BaseResultDTO
{
    public ApPaymentSnapshotDTO? Payment { get; set; }
    public IReadOnlyList<ApInvoiceSettlementSnapshotDTO> Invoices { get; set; } = [];

    public static ApPaymentCommandResultDTO SuccessWith(
        ApPaymentSnapshotDTO payment,
        IReadOnlyList<ApInvoiceSettlementSnapshotDTO> invoices,
        string message
    ) =>
        new()
        {
            Success = true,
            Payment = payment,
            Invoices = invoices,
            Message = message,
        };

    public static ApPaymentCommandResultDTO Fail(string message, HttpStatusCode statusCode) =>
        Fail<ApPaymentCommandResultDTO>(message, statusCode);
}
