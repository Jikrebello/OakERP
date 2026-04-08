using OakERP.Common.Enums;

namespace OakERP.Application.AccountsReceivable.Invoices.Support;

public static class ArInvoiceCommandValidator
{
    public static ArInvoiceCreateValidationResult ValidateCreate(CreateArInvoiceCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        string docNo = command.DocNo.Trim();
        string currencyCode = NormalizeCurrencyCode(command.CurrencyCode);
        string? shipTo = NormalizeOptional(command.ShipTo);
        string? memo = NormalizeOptional(command.Memo);
        string performedBy = GetPerformedBy(command.PerformedBy);
        List<ValidatedArInvoiceLineInput> lines = NormalizeLines(command.Lines ?? []);

        return new ArInvoiceCreateValidationResult(
            ValidateRequest(command, docNo, currencyCode, shipTo, memo, lines),
            docNo,
            currencyCode,
            shipTo,
            memo,
            performedBy,
            lines
        );
    }

    private static ArInvoiceCommandResultDto? ValidateRequest(
        CreateArInvoiceCommand command,
        string docNo,
        string currencyCode,
        string? shipTo,
        string? memo,
        IReadOnlyList<ValidatedArInvoiceLineInput> lines
    ) =>
        ValidateDocumentNumber(docNo)
        ?? ValidateCustomer(command.CustomerId)
        ?? ValidateInvoiceDates(command.InvoiceDate, command.DueDate)
        ?? ValidateShipTo(shipTo)
        ?? ValidateMemo(memo)
        ?? ValidateCurrencyCode(currencyCode)
        ?? ValidateTotals(command.TaxTotal, command.DocTotal, lines)
        ?? ValidateLines(lines);

    private static ArInvoiceCommandResultDto? ValidateDocumentNumber(string docNo)
    {
        if (string.IsNullOrWhiteSpace(docNo))
        {
            return Fail(ArInvoiceErrors.DocumentNumberRequired);
        }

        return docNo.Length > 40 ? Fail(ArInvoiceErrors.DocumentNumberTooLong) : null;
    }

    private static ArInvoiceCommandResultDto? ValidateCustomer(Guid customerId) =>
        customerId == Guid.Empty ? Fail(ArInvoiceErrors.CustomerIdRequired) : null;

    private static ArInvoiceCommandResultDto? ValidateInvoiceDates(
        DateOnly invoiceDate,
        DateOnly? dueDate
    )
    {
        if (invoiceDate == default)
        {
            return Fail(ArInvoiceErrors.InvoiceDateRequired);
        }

        return dueDate is not null && dueDate.Value < invoiceDate
            ? Fail(ArInvoiceErrors.DueDateBeforeInvoiceDate)
            : null;
    }

    private static ArInvoiceCommandResultDto? ValidateShipTo(string? shipTo) =>
        shipTo is not null && shipTo.Length > 512 ? Fail(ArInvoiceErrors.ShipToTooLong) : null;

    private static ArInvoiceCommandResultDto? ValidateMemo(string? memo) =>
        memo is not null && memo.Length > 512 ? Fail(ArInvoiceErrors.MemoTooLong) : null;

    private static ArInvoiceCommandResultDto? ValidateCurrencyCode(string currencyCode) =>
        currencyCode.Length != 3 ? Fail(ArInvoiceErrors.CurrencyCodeInvalid) : null;

    private static ArInvoiceCommandResultDto? ValidateTotals(
        decimal taxTotal,
        decimal docTotal,
        IReadOnlyList<ValidatedArInvoiceLineInput> lines
    )
    {
        if (taxTotal < 0m)
        {
            return Fail(ArInvoiceErrors.TaxTotalNegative);
        }

        if (docTotal < 0m)
        {
            return Fail(ArInvoiceErrors.DocumentTotalNegative);
        }

        if (lines.Count == 0)
        {
            return Fail(ArInvoiceErrors.InvoiceLineRequired);
        }

        decimal computedDocTotal = lines.Sum(x => x.LineTotal) + taxTotal;
        return computedDocTotal != docTotal ? Fail(ArInvoiceErrors.DocumentTotalMismatch) : null;
    }

    private static ArInvoiceCommandResultDto? ValidateLines(
        IReadOnlyList<ValidatedArInvoiceLineInput> lines
    )
    {
        foreach (ValidatedArInvoiceLineInput line in lines)
        {
            if (line.Description is not null && line.Description.Length > 512)
            {
                return Fail(ArInvoiceErrors.LineDescriptionTooLong);
            }

            if (line.RevenueAccount is not null && line.RevenueAccount.Length > 20)
            {
                return Fail(ArInvoiceErrors.LineRevenueAccountTooLong);
            }

            if (line.ItemId is null && string.IsNullOrWhiteSpace(line.RevenueAccount))
            {
                return Fail(ArInvoiceErrors.LineRequiresRevenueAccountOrItem);
            }

            if (line.Qty < 0m || line.UnitPrice < 0m || line.LineTotal < 0m)
            {
                return Fail(ArInvoiceErrors.LineAmountsNegative);
            }
        }

        return null;
    }

    private static List<ValidatedArInvoiceLineInput> NormalizeLines(
        IReadOnlyList<ArInvoiceLineInputDto> lines
    ) =>
        lines
            .Select(x => new ValidatedArInvoiceLineInput(
                NormalizeOptional(x.Description),
                NormalizeOptional(x.RevenueAccount),
                x.ItemId,
                x.Qty,
                x.UnitPrice,
                x.TaxRateId,
                x.LocationId,
                x.LineTotal
            ))
            .ToList();

    private static string NormalizeCurrencyCode(string? currencyCode) =>
        string.IsNullOrWhiteSpace(currencyCode)
            ? CurrencyIsoCodes.ZAR.ToString()
            : currencyCode.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GetPerformedBy(string? performedBy) =>
        string.IsNullOrWhiteSpace(performedBy) ? "system" : performedBy.Trim();

    private static ArInvoiceCommandResultDto Fail(OakERP.Common.Errors.ResultError error) =>
        ArInvoiceCommandResultDto.Fail(error);
}

public sealed record ArInvoiceCreateValidationResult(
    ArInvoiceCommandResultDto? Failure,
    string DocNo,
    string CurrencyCode,
    string? ShipTo,
    string? Memo,
    string PerformedBy,
    IReadOnlyList<ValidatedArInvoiceLineInput> Lines
);

public sealed record ValidatedArInvoiceLineInput(
    string? Description,
    string? RevenueAccount,
    Guid? ItemId,
    decimal Qty,
    decimal UnitPrice,
    Guid? TaxRateId,
    Guid? LocationId,
    decimal LineTotal
);
