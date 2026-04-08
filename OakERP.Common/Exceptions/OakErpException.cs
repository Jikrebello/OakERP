using System.Net;

namespace OakERP.Common.Exceptions;

public abstract class OakErpException(
    string message,
    HttpStatusCode statusCode,
    string title,
    Exception? innerException = null
) : Exception(message, innerException)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string Title { get; } = title;
}
