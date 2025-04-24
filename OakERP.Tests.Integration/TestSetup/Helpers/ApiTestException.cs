using System.Net;

namespace OakERP.Tests.Integration.TestSetup.Helpers;

public class ApiTestException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseBody { get; }

    public ApiTestException(string message, HttpStatusCode statusCode, string? body)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = body;
    }
}
