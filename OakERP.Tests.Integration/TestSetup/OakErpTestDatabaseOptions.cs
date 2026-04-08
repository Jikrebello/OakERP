using OakERP.Common.Exceptions;

namespace OakERP.Tests.Integration.TestSetup;

internal sealed class OakErpTestDatabaseOptions
{
    public string? TransactionalConnectionString { get; init; }

    public string? ResetConnectionString { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TransactionalConnectionString))
        {
            throw new ConfigurationValidationException(
                "Tests:TransactionalConnectionString",
                "Tests:TransactionalConnectionString is not configured."
            );
        }

        if (string.IsNullOrWhiteSpace(ResetConnectionString))
        {
            throw new ConfigurationValidationException(
                "Tests:ResetConnectionString",
                "Tests:ResetConnectionString is not configured."
            );
        }
    }
}
