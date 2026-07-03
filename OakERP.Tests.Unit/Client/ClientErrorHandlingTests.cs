using Microsoft.Extensions.Logging.Abstractions;
using OakERP.Client.Services.Api;
using OakERP.Client.Services.Errors;
using OakERP.UI.Errors;
using OakERP.UI.ViewModels.Support;
using Shouldly;

namespace OakERP.Tests.Unit.ClientErrorHandling;

public sealed class ClientErrorHandlingTests
{
    [Fact]
    public async Task UiOperationRunner_Should_Reset_Busy_State_When_Operation_Throws()
    {
        var handler = new CapturingClientErrorHandler();
        var runner = new UiOperationRunner(handler);
        var isBusy = false;
        var operationSawBusy = false;

        await runner.RunBusyAsync(
            () =>
            {
                operationSawBusy = isBusy;
                throw new InvalidOperationException("boom");
            },
            value => isBusy = value,
            "TestOperation"
        );

        operationSawBusy.ShouldBeTrue();
        isBusy.ShouldBeFalse();
        handler.Exception.ShouldBeOfType<InvalidOperationException>();
        handler.Context.ShouldBe(new ClientErrorContext("TestOperation"));
    }

    [Fact]
    public async Task ToastClientErrorHandler_Should_Show_Generic_Error_Notification()
    {
        var notifier = new CapturingUiErrorNotifier();
        var handler = new ToastClientErrorHandler(
            NullLogger<ToastClientErrorHandler>.Instance,
            notifier
        );

        var result = await handler.HandleAsync(
            new InvalidOperationException("boom"),
            new ClientErrorContext("Render")
        );

        result.ShouldBe(ClientErrorResult.Generic);
        notifier.ShowUnexpectedErrorCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task ApiClient_Should_Route_Transport_Exception_Through_Client_Error_Handler()
    {
        var handler = new CapturingClientErrorHandler();
        var apiClient = new ApiClient(
            new HttpClient(new ThrowingHttpMessageHandler())
            {
                BaseAddress = new Uri("https://example.test/"),
            },
            NullLogger<ApiClient>.Instance,
            handler
        );

        var result = await apiClient.GetAsync<object>("api/test");

        result.Success.ShouldBeFalse();
        result.Message.ShouldBe(ClientErrorResult.Generic.Message);
        result.StatusCode.ShouldBe(ClientErrorResult.Generic.StatusCode);
        handler.Exception.ShouldBeOfType<InvalidOperationException>();
        handler.Context.ShouldBe(new ClientErrorContext("GET", "api/test"));
    }

    private sealed class CapturingClientErrorHandler : IClientErrorHandler
    {
        public Exception? Exception { get; private set; }

        public ClientErrorContext? Context { get; private set; }

        public ValueTask<ClientErrorResult> HandleAsync(
            Exception exception,
            ClientErrorContext context,
            CancellationToken cancellationToken = default
        )
        {
            Exception = exception;
            Context = context;
            return ValueTask.FromResult(ClientErrorResult.Generic);
        }
    }

    private sealed class CapturingUiErrorNotifier : IUiErrorNotifier
    {
        public int ShowUnexpectedErrorCallCount { get; private set; }

        public void ShowUnexpectedError()
        {
            ShowUnexpectedErrorCallCount++;
        }
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            throw new InvalidOperationException("transport failed");
        }
    }
}
