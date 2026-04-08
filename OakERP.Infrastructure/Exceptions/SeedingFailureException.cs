using OakERP.Common.Errors;
using OakERP.Common.Exceptions;

namespace OakERP.Infrastructure.Exceptions;

public sealed class SeedingFailureException(string message, Exception? innerException = null)
    : OakErpException(
        message,
        FailureKind.Unexpected,
        "Database seeding failed.",
        innerException
    );
