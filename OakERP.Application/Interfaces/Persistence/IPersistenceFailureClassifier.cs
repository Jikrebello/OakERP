namespace OakERP.Application.Interfaces.Persistence;

public interface IPersistenceFailureClassifier
{
    bool IsUniqueConstraint(Exception exception, string constraintName);

    bool IsApInvoiceDocNoConflict(Exception exception);

    bool IsApInvoiceVendorInvoiceNoConflict(Exception exception);

    bool IsApPaymentDocNoConflict(Exception exception);

    bool IsArInvoiceDocNoConflict(Exception exception);

    bool IsArReceiptDocNoConflict(Exception exception);

    bool IsConcurrencyConflict(Exception exception);
}
