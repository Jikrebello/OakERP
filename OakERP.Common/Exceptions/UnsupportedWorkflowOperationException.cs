using System.Net;

namespace OakERP.Common.Exceptions;

public sealed class UnsupportedWorkflowOperationException(string message)
    : OakErpException(message, HttpStatusCode.BadRequest, "The requested operation is not supported.");
