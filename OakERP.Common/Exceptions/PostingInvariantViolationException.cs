using System.Net;

namespace OakERP.Common.Exceptions;

public sealed class PostingInvariantViolationException(string message, Exception? innerException = null)
    : OakErpException(message, HttpStatusCode.InternalServerError, "Posting invariant was violated.", innerException);
