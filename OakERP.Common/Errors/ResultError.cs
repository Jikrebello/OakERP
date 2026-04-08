using System.Net;

namespace OakERP.Common.Errors;

public sealed record ResultError(string Message, HttpStatusCode StatusCode);
