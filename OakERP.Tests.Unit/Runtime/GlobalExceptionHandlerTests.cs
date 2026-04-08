using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using OakERP.API.Extensions;
using Shouldly;

namespace OakERP.Tests.Unit.Runtime;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Should_Map_OakErpException_To_ProblemDetails()
    {
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = CreateHandler(problemDetailsService, Environments.Production);
        var context = CreateHttpContext();

        bool handled = await handler.TryHandleAsync(
            context,
            new ResourceNotFoundException("AP invoice", "INV-1001"),
            CancellationToken.None
        );

        handled.ShouldBeTrue();
        problemDetailsService.LastContext.ShouldNotBeNull();
        problemDetailsService.LastContext.ProblemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
        problemDetailsService.LastContext.ProblemDetails.Title.ShouldBe("AP invoice was not found.");
        problemDetailsService.LastContext.ProblemDetails.Detail.ShouldBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_Should_Expose_Detail_For_OakErpException_In_Development()
    {
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = CreateHandler(problemDetailsService, Environments.Development);
        var context = CreateHttpContext();

        bool handled = await handler.TryHandleAsync(
            context,
            new ConfigurationValidationException("Api:BaseUrl", "Api:BaseUrl is not configured."),
            CancellationToken.None
        );

        handled.ShouldBeTrue();
        problemDetailsService.LastContext.ShouldNotBeNull();
        problemDetailsService.LastContext.ProblemDetails.Status.ShouldBe(StatusCodes.Status500InternalServerError);
        problemDetailsService.LastContext.ProblemDetails.Title.ShouldBe("Application configuration is invalid.");
        problemDetailsService.LastContext.ProblemDetails.Detail.ShouldBe("Api:BaseUrl is not configured.");
    }

    [Fact]
    public async Task TryHandleAsync_Should_Map_Unknown_Exception_To_Generic_ProblemDetails()
    {
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = CreateHandler(problemDetailsService, Environments.Production);
        var context = CreateHttpContext();

        bool handled = await handler.TryHandleAsync(
            context,
            new Exception("boom"),
            CancellationToken.None
        );

        handled.ShouldBeTrue();
        problemDetailsService.LastContext.ShouldNotBeNull();
        problemDetailsService.LastContext.ProblemDetails.Status.ShouldBe(StatusCodes.Status500InternalServerError);
        problemDetailsService.LastContext.ProblemDetails.Title.ShouldBe("An unexpected error occurred.");
        problemDetailsService.LastContext.ProblemDetails.Detail.ShouldBeNull();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/test";
        return context;
    }

    private static GlobalExceptionHandler CreateHandler(
        CapturingProblemDetailsService problemDetailsService,
        string environmentName
    )
    {
        return new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance,
            problemDetailsService,
            new TestHostEnvironment { EnvironmentName = environmentName }
        );
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext LastContext { get; private set; } = default!;

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            LastContext = context;
            return ValueTask.FromResult(true);
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "OakERP.Tests.Unit";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}
