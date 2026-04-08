using Microsoft.EntityFrameworkCore;
using Npgsql;
using OakERP.Application.Interfaces.Persistence;

namespace OakERP.Infrastructure.Persistence;

public sealed class PersistenceFailureClassifier : IPersistenceFailureClassifier
{
    public bool IsUniqueConstraint(Exception exception, string constraintName) =>
        exception is DbUpdateException dbUpdateException
        && dbUpdateException.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
        && string.Equals(
            postgresException.ConstraintName,
            constraintName,
            StringComparison.Ordinal
        );

    public bool IsConcurrencyConflict(Exception exception) =>
        exception is DbUpdateConcurrencyException;
}
