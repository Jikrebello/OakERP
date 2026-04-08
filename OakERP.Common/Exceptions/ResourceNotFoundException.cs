using System.Net;

namespace OakERP.Common.Exceptions;

public sealed class ResourceNotFoundException(string resourceName, string resourceId)
    : OakErpException(
        $"{resourceName} '{resourceId}' was not found.",
        HttpStatusCode.NotFound,
        $"{resourceName} was not found."
    );
