using Microsoft.EntityFrameworkCore;
using Npgsql;
using OakERP.Application.Interfaces.Persistence;

namespace OakERP.Infrastructure.Persistence;

public sealed class PersistenceFailureClassifier : IPersistenceFailureClassifier
{
    private const string ApInvoiceDocNoConstraint = "ix_ap_invoices_doc_no";
    private const string ApInvoiceVendorInvoiceNoConstraint = "ix_ap_invoices_vendor_id_invoice_no";
    private const string ApPaymentDocNoConstraint = "ix_ap_payments_doc_no";
    private const string ArReceiptDocNoConstraint = "ix_ar_receipts_doc_no";

    public bool IsUniqueConstraint(Exception exception, string constraintName) =>
        exception is DbUpdateException dbUpdateException
        && dbUpdateException.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(
            postgresException.ConstraintName,
            constraintName,
            StringComparison.Ordinal
        );

    public bool IsApInvoiceDocNoConflict(Exception exception) =>
        IsUniqueConstraint(exception, ApInvoiceDocNoConstraint);

    public bool IsApInvoiceVendorInvoiceNoConflict(Exception exception) =>
        IsUniqueConstraint(exception, ApInvoiceVendorInvoiceNoConstraint);

    public bool IsApPaymentDocNoConflict(Exception exception) =>
        IsUniqueConstraint(exception, ApPaymentDocNoConstraint);

    public bool IsArReceiptDocNoConflict(Exception exception) =>
        IsUniqueConstraint(exception, ArReceiptDocNoConstraint);

    public bool IsConcurrencyConflict(Exception exception) =>
        exception is DbUpdateConcurrencyException;
}
