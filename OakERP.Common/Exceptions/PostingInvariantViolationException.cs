using OakERP.Common.Errors;

namespace OakERP.Common.Exceptions;

public sealed class PostingInvariantViolationException : OakErpException
{
    public PostingInvariantViolationException(string message, Exception? innerException = null)
        : this(message, FailureKind.Unexpected, innerException) { }

    public PostingInvariantViolationException(
        string message,
        FailureKind failureKind,
        Exception? innerException = null
    )
        : base(message, failureKind, "Posting invariant was violated.", innerException) { }
}
