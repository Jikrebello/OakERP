using System.Net;
using OakERP.Common.Enums;

namespace OakERP.Application.AccountsPayable.Invoices.Support;

public static class ApInvoiceCommandValidator
{
    public static ApInvoiceCreateValidationResult ValidateCreate(CreateApInvoiceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        string docNo = command.DocNo.Trim();
        string invoiceNo = command.InvoiceNo.Trim();
        string currencyCode = NormalizeCurrencyCode(command.CurrencyCode);
        string? memo = NormalizeOptional(command.Memo);
        string performedBy = GetPerformedBy(command.PerformedBy);
        List<ValidatedApInvoiceLineInput> lines = NormalizeLines(command.Lines ?? []);

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

    private static ApInvoiceCommandResultDto? ValidateRequest(
        CreateApInvoiceCommand command,
        string docNo,
        string invoiceNo,
        string currencyCode,
        string? memo,
        IReadOnlyList<ValidatedApInvoiceLineInput> lines
    ) =>
        ValidateDocumentNumber(docNo)
        ?? ValidateVendor(command.VendorId)
        ?? ValidateInvoiceNumber(invoiceNo)
        ?? ValidateInvoiceDates(command.InvoiceDate, command.DueDate)
        ?? ValidateMemo(memo)
        ?? ValidateCurrencyCode(currencyCode)
        ?? ValidateTotals(command.TaxTotal, command.DocTotal, lines)
        ?? ValidateLines(lines);

    private static ApInvoiceCommandResultDto? ValidateDocumentNumber(string docNo)
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return Fail("Document number is required.");
        }

        return docNo.Length > 40 ? Fail("Document number may not exceed 40 characters.") : null;
    }

    private static ApInvoiceCommandResultDto? ValidateVendor(Guid vendorId) =>
        vendorId == Guid.Empty ? Fail("Vendor id is required.") : null;

    private static ApInvoiceCommandResultDto? ValidateInvoiceNumber(string invoiceNo)
    {
        if (string.IsNullOrWhiteSpace(invoiceNo))
        {
            return Fail("Vendor invoice number is required.");
        }

        return invoiceNo.Length > 40
            ? Fail("Vendor invoice number may not exceed 40 characters.")
            : null;
    }

    private static ApInvoiceCommandResultDto? ValidateInvoiceDates(
        DateOnly invoiceDate,
        DateOnly? dueDate
    )
    {
        if (invoiceDate == default)
        {
            return Fail("Invoice date is required.");
        }

        return dueDate is not null && dueDate.Value < invoiceDate
            ? Fail("Due date may not be earlier than the invoice date.")
            : null;
    }

    private static ApInvoiceCommandResultDto? ValidateMemo(string? memo) =>
        memo is not null && memo.Length > 512
            ? Fail("Invoice memo may not exceed 512 characters.")
            : null;

    private static ApInvoiceCommandResultDto? ValidateCurrencyCode(string currencyCode) =>
        currencyCode.Length != 3 ? Fail("Currency code must be a 3-character ISO code.") : null;

    private static ApInvoiceCommandResultDto? ValidateTotals(
        decimal taxTotal,
        decimal docTotal,
        IReadOnlyList<ValidatedApInvoiceLineInput> lines
    )
    {
        if (taxTotal < 0m)
        {
            return Fail("Tax total may not be negative.");
        }

        if (docTotal < 0m)
        {
            return Fail("Document total may not be negative.");
        }

        if (lines.Count == 0)
        {
            return Fail("At least one invoice line is required.");
        }

        decimal computedDocTotal = lines.Sum(x => x.LineTotal) + taxTotal;
        return computedDocTotal != docTotal
            ? Fail("Document total must equal the sum of line totals plus tax total.")
            : null;
    }

    private static ApInvoiceCommandResultDto? ValidateLines(
        IReadOnlyList<ValidatedApInvoiceLineInput> lines
    )
    {
        foreach (ValidatedApInvoiceLineInput line in lines)
        {
            if (line.ItemId is not null)
            {
                return Fail("Item-based AP invoice lines are deferred in this slice.");
            }

            if (line.TaxRateId is not null)
            {
                return Fail("Tax-rated AP invoice lines are deferred in this slice.");
            }

            if (string.IsNullOrWhiteSpace(line.AccountNo))
            {
                return Fail("Each AP invoice line must specify a GL account.");
            }

            if (line.AccountNo.Length > 20)
            {
                return Fail("Line account number may not exceed 20 characters.");
            }

            if (line.Description is not null && line.Description.Length > 512)
            {
                return Fail("Line description may not exceed 512 characters.");
            }

            if (line.Qty < 0m || line.UnitPrice < 0m || line.LineTotal < 0m)
            {
                return Fail("Line quantities and amounts may not be negative.");
            }
        }

        return null;
    }

    private static List<ValidatedApInvoiceLineInput> NormalizeLines(
        IReadOnlyList<ApInvoiceLineInputDto> lines
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
            ? CurrencyIsoCodes.ZAR.ToString()
            : currencyCode.Trim().ToUpperInvariant();

    private static ApInvoiceCommandResultDto Fail(string message) =>
        ApInvoiceCommandResultDto.Fail(message, HttpStatusCode.BadRequest);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GetPerformedBy(string? performedBy) =>
        string.IsNullOrWhiteSpace(performedBy) ? "system" : performedBy.Trim();
}

public sealed record ApInvoiceCreateValidationResult(
    ApInvoiceCommandResultDto? Failure,
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
