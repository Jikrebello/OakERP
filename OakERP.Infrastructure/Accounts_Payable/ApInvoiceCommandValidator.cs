using System.Net;
using OakERP.Application.AccountsPayable;
using OakERP.Common.Enums;

namespace OakERP.Infrastructure.Accounts_Payable;

public sealed class ApInvoiceCommandValidator
{
    public ApInvoiceCreateValidationResult ValidateCreate(CreateApInvoiceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        string docNo = command.DocNo.Trim();
        string invoiceNo = command.InvoiceNo.Trim();
        string currencyCode = NormalizeCurrencyCode(command.CurrencyCode);
        string? memo = NormalizeOptional(command.Memo);
        string performedBy = GetPerformedBy(command.PerformedBy);
        IReadOnlyList<ValidatedApInvoiceLineInput> lines = NormalizeLines(command.Lines ?? []);

        return new ApInvoiceCreateValidationResult(
            ValidateRequest(command, docNo, invoiceNo, currencyCode, memo, lines),
            docNo,
            invoiceNo,
            currencyCode,
            memo,
            performedBy,
            lines
        );
    }

    private static ApInvoiceCommandResultDTO? ValidateRequest(
        CreateApInvoiceCommand command,
        string docNo,
        string invoiceNo,
        string currencyCode,
        string? memo,
        IReadOnlyList<ValidatedApInvoiceLineInput> lines
    )
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Document number is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (docNo.Length > 40)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Document number may not exceed 40 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.VendorId == Guid.Empty)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Vendor id is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (string.IsNullOrWhiteSpace(invoiceNo))
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Vendor invoice number is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (invoiceNo.Length > 40)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Vendor invoice number may not exceed 40 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.InvoiceDate == default)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Invoice date is required.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.DueDate is not null && command.DueDate.Value < command.InvoiceDate)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Due date may not be earlier than the invoice date.",
                HttpStatusCode.BadRequest
            );
        }

        if (memo is not null && memo.Length > 512)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Invoice memo may not exceed 512 characters.",
                HttpStatusCode.BadRequest
            );
        }

        if (currencyCode.Length != 3)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Currency code must be a 3-character ISO code.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.TaxTotal < 0m)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Tax total may not be negative.",
                HttpStatusCode.BadRequest
            );
        }

        if (command.DocTotal < 0m)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Document total may not be negative.",
                HttpStatusCode.BadRequest
            );
        }

        if (lines.Count == 0)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "At least one invoice line is required.",
                HttpStatusCode.BadRequest
            );
        }

        decimal computedDocTotal = lines.Sum(x => x.LineTotal) + command.TaxTotal;
        if (computedDocTotal != command.DocTotal)
        {
            return ApInvoiceCommandResultDTO.Fail(
                "Document total must equal the sum of line totals plus tax total.",
                HttpStatusCode.BadRequest
            );
        }

        foreach (ValidatedApInvoiceLineInput line in lines)
        {
            if (line.ItemId is not null)
            {
                return ApInvoiceCommandResultDTO.Fail(
                    "Item-based AP invoice lines are deferred in this slice.",
                    HttpStatusCode.BadRequest
                );
            }

            if (line.TaxRateId is not null)
            {
                return ApInvoiceCommandResultDTO.Fail(
                    "Tax-rated AP invoice lines are deferred in this slice.",
                    HttpStatusCode.BadRequest
                );
            }

            if (string.IsNullOrWhiteSpace(line.AccountNo))
            {
                return ApInvoiceCommandResultDTO.Fail(
                    "Each AP invoice line must specify a GL account.",
                    HttpStatusCode.BadRequest
                );
            }

            if (line.AccountNo.Length > 20)
            {
                return ApInvoiceCommandResultDTO.Fail(
                    "Line account number may not exceed 20 characters.",
                    HttpStatusCode.BadRequest
                );
            }

            if (line.Description is not null && line.Description.Length > 512)
            {
                return ApInvoiceCommandResultDTO.Fail(
                    "Line description may not exceed 512 characters.",
                    HttpStatusCode.BadRequest
                );
            }

            if (line.Qty < 0m || line.UnitPrice < 0m || line.LineTotal < 0m)
            {
                return ApInvoiceCommandResultDTO.Fail(
                    "Line quantities and amounts may not be negative.",
                    HttpStatusCode.BadRequest
                );
            }
        }

        return null;
    }

    private static IReadOnlyList<ValidatedApInvoiceLineInput> NormalizeLines(
        IReadOnlyList<ApInvoiceLineInputDTO> lines
    ) =>
        lines
            .Select(x => new ValidatedApInvoiceLineInput(
                NormalizeOptional(x.Description),
                NormalizeOptional(x.AccountNo),
                x.ItemId,
                x.Qty,
                x.UnitPrice,
                x.TaxRateId,
                x.LineTotal
            ))
            .ToList();

    private static string NormalizeCurrencyCode(string? currencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode)
            ? CurrencyISOCodes.ZAR.ToString()
            : currencyCode.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GetPerformedBy(string? performedBy) =>
        string.IsNullOrWhiteSpace(performedBy) ? "system" : performedBy.Trim();
}

public sealed record ApInvoiceCreateValidationResult(
    ApInvoiceCommandResultDTO? Failure,
    string DocNo,
    string InvoiceNo,
    string CurrencyCode,
    string? Memo,
    string PerformedBy,
    IReadOnlyList<ValidatedApInvoiceLineInput> Lines
);

public sealed record ValidatedApInvoiceLineInput(
    string? Description,
    string? AccountNo,
    Guid? ItemId,
    decimal Qty,
    decimal UnitPrice,
    Guid? TaxRateId,
    decimal LineTotal
);
