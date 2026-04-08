using OakERP.Common.Errors;

namespace OakERP.Common.Exceptions;

public sealed class UnsupportedWorkflowOperationException(string message)
    : OakErpException(message, FailureKind.Validation, "The requested operation is not supported.");
