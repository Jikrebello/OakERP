namespace OakERP.Application.Interfaces.Persistence;

public interface IPersistenceFailureClassifier
{
    bool IsUniqueConstraint(Exception exception, string constraintName);

    bool IsConcurrencyConflict(Exception exception);
}
