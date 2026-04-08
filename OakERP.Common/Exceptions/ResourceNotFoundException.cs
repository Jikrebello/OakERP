using OakERP.Common.Errors;

namespace OakERP.Common.Exceptions;

public sealed class ResourceNotFoundException(string resourceName, string resourceId)
    : OakErpException(
        $"{resourceName} '{resourceId}' was not found.",
        FailureKind.NotFound,
        $"{resourceName} was not found."
    );
