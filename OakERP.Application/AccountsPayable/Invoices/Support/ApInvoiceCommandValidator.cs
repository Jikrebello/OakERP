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
            return Fail(ApInvoiceErrors.DocumentNumberRequired);
        }

        return docNo.Length > 40 ? Fail(ApInvoiceErrors.DocumentNumberTooLong) : null;
    }

    private static ApInvoiceCommandResultDto? ValidateVendor(Guid vendorId) =>
        vendorId == Guid.Empty ? Fail(ApInvoiceErrors.VendorIdRequired) : null;

    private static ApInvoiceCommandResultDto? ValidateInvoiceNumber(string invoiceNo)
    {
        if (string.IsNullOrWhiteSpace(invoiceNo))
        {
            return Fail(ApInvoiceErrors.VendorInvoiceNumberRequired);
        }

        return invoiceNo.Length > 40 ? Fail(ApInvoiceErrors.VendorInvoiceNumberTooLong) : null;
    }

    private static ApInvoiceCommandResultDto? ValidateInvoiceDates(
        DateOnly invoiceDate,
        DateOnly? dueDate
    )
    {
        if (invoiceDate == default)
        {
            return Fail(ApInvoiceErrors.InvoiceDateRequired);
        }

        return dueDate is not null && dueDate.Value < invoiceDate
            ? Fail(ApInvoiceErrors.DueDateBeforeInvoiceDate)
            : null;
    }

    private static ApInvoiceCommandResultDto? ValidateMemo(string? memo) =>
        memo is not null && memo.Length > 512 ? Fail(ApInvoiceErrors.MemoTooLong) : null;

    private static ApInvoiceCommandResultDto? ValidateCurrencyCode(string currencyCode) =>
        currencyCode.Length != 3 ? Fail(ApInvoiceErrors.CurrencyCodeInvalid) : null;

    private static ApInvoiceCommandResultDto? ValidateTotals(
        decimal taxTotal,
        decimal docTotal,
        IReadOnlyList<ValidatedApInvoiceLineInput> lines
    )
    {
        if (taxTotal < 0m)
        {
            return Fail(ApInvoiceErrors.TaxTotalNegative);
        }

        if (docTotal < 0m)
        {
            return Fail(ApInvoiceErrors.DocumentTotalNegative);
        }

        if (lines.Count == 0)
        {
            return Fail(ApInvoiceErrors.InvoiceLineRequired);
        }

        decimal computedDocTotal = lines.Sum(x => x.LineTotal) + taxTotal;
        return computedDocTotal != docTotal ? Fail(ApInvoiceErrors.DocumentTotalMismatch) : null;
    }

    private static ApInvoiceCommandResultDto? ValidateLines(
        IReadOnlyList<ValidatedApInvoiceLineInput> lines
    )
    {
        foreach (ValidatedApInvoiceLineInput line in lines)
        {
            if (line.ItemId is not null)
            {
                return Fail(ApInvoiceErrors.ItemLinesDeferred);
            }

            if (line.TaxRateId is not null)
            {
                return Fail(ApInvoiceErrors.TaxRatedLinesDeferred);
            }

            if (string.IsNullOrWhiteSpace(line.AccountNo))
            {
                return Fail(ApInvoiceErrors.LineAccountRequired);
            }

            if (line.AccountNo.Length > 20)
            {
                return Fail(ApInvoiceErrors.LineAccountTooLong);
            }

            if (line.Description is not null && line.Description.Length > 512)
            {
                return Fail(ApInvoiceErrors.LineDescriptionTooLong);
            }

            if (line.Qty < 0m || line.UnitPrice < 0m || line.LineTotal < 0m)
            {
                return Fail(ApInvoiceErrors.LineAmountsNegative);
            }
        }

        return null;
    }

    private static List<ValidatedApInvoiceLineInput> NormalizeLines(
        IReadOnlyList<ApInvoiceLineInputDto> lines
    ) =>
        [
            .. lines.Select(x => new ValidatedApInvoiceLineInput(
                NormalizeOptional(x.Description),
                NormalizeOptional(x.AccountNo),
                x.ItemId,
                x.Qty,
                x.UnitPrice,
                x.TaxRateId,
                x.LineTotal
            )),
        ];

    private static string NormalizeCurrencyCode(string? currencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode)
            ? CurrencyIsoCodes.ZAR.ToString()
            : currencyCode.Trim().ToUpperInvariant();

    private static ApInvoiceCommandResultDto Fail(OakERP.Common.Errors.ResultError error) =>
        ApInvoiceCommandResultDto.Fail(error);

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
