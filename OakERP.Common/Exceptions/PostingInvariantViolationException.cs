using OakERP.Common.Errors;

namespace OakERP.Common.Exceptions;

public sealed class PostingInvariantViolationException(string message, Exception? innerException = null)
    : OakErpException(
        message,
        FailureKind.Unexpected,
        "Posting invariant was violated.",
        innerException
    );
