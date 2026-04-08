using System.Net;
using OakERP.Common.Dtos.Base;

namespace OakERP.Application.AccountsPayable;

public sealed class ApPaymentCommandResultDto : BaseResultDto
{
    public ApPaymentSnapshotDto? Payment { get; set; }
    public IReadOnlyList<ApInvoiceSettlementSnapshotDto> Invoices { get; set; } = [];

    public static ApPaymentCommandResultDto SuccessWith(
        ApPaymentSnapshotDto payment,
        IReadOnlyList<ApInvoiceSettlementSnapshotDto> invoices,
        string message
    ) =>
        new()
        {
            Success = true,
            Payment = payment,
            Invoices = invoices,
            Message = message,
        };

    public static ApPaymentCommandResultDto Fail(string message, HttpStatusCode statusCode) =>
        Fail<ApPaymentCommandResultDto>(message, statusCode);
}
