using System.Net;

namespace OakERP.Common.Exceptions;

public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : OakErpException(message, HttpStatusCode.Conflict, "The resource was modified by another operation.", innerException);
