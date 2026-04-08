using System.Net;
using OakERP.Common.Exceptions;

namespace OakERP.Infrastructure.Exceptions;

public sealed class SeedingFailureException(string message, Exception? innerException = null)
    : OakErpException(message, HttpStatusCode.InternalServerError, "Database seeding failed.", innerException);
