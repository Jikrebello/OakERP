using OakERP.Common.Errors;

namespace OakERP.Common.Exceptions;

public abstract class OakErpException(
    string message,
    FailureKind failureKind,
    string title,
    Exception? innerException = null
) : Exception(message, innerException)
{
    public FailureKind FailureKind { get; } = failureKind;

    public string Title { get; } = title;
}
