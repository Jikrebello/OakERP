using System.Net;
using OakERP.Common.Exceptions;

namespace OakERP.Infrastructure.Exceptions;

public sealed class EmbeddedResourceNotFoundException(string resourcePath)
    : OakErpException(
        $"Embedded resource '{resourcePath}' was not found.",
        HttpStatusCode.InternalServerError,
        "A required embedded resource is missing."
    );
