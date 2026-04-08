using OakERP.Common.Errors;

namespace OakERP.Common.Exceptions;

public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : OakErpException(
        message,
        FailureKind.Conflict,
        "The resource was modified by another operation.",
        innerException
    );
