using OakERP.Common.Errors;
using OakERP.Common.Exceptions;

namespace OakERP.Infrastructure.Exceptions;

public sealed class EmbeddedResourceNotFoundException(string resourcePath)
    : OakErpException(
        $"Embedded resource '{resourcePath}' was not found.",
        FailureKind.Unexpected,
        "A required embedded resource is missing."
    );
